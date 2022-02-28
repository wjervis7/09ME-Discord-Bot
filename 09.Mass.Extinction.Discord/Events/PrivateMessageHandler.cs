namespace _09.Mass.Extinction.Discord.Events;

using Data;
using Data.Entities;
using global::Discord;
using global::Discord.Net;
using global::Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class PrivateMessageHandler : IMessageHandler
{
    private const string _noBodyText = @"
Your message lacks content, so I can't send anything to the Admin Team. Perhaps, you've included:
• GIFs
• Images
• Stickers
If so, edit your message, and try sending it, again.
";

    private const string _anonymousReplyMessageText = "Do you want to send this anonymously? React with 🇾 for yes, or 🇳 for no.";
    private const string _messageSentText = "Message was sent to the admin team.";
    private const string _yesEmoji = "🇾";
    private const string _noEmoji = "🇳";

    private readonly DiscordSocketClient _client;
    private readonly ILogger<PrivateMessageHandler> _logger;
    private readonly Dictionary<ulong, SocketUserMessage> _pendingMessages = new();
    private readonly IServiceProvider _serviceProvider;

    public PrivateMessageHandler(ILogger<PrivateMessageHandler> logger, DiscordSocketClient client, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _client = client;
        _serviceProvider = serviceProvider;

        _client.ReactionAdded += OnReactionAdded;
    }

    public string Name => "Direct Message Handler";

    public async Task Handle(SocketUserMessage message)
    {
        _logger.LogDebug("Message received by {messageHandler}.", Name);
        if (message.Channel.GetChannelType() != ChannelType.DM || message.Author.IsBot)
        {
            _logger.LogDebug("Message is not a DM, or, author is a bot; message not meant for this handler.");
            return;
        }

        if (message.Content.Length == 0)
        {
            _logger.LogDebug("Content length is 0, so nothing to send, or contains stuff that can't be sent.");
            await message.ReplyAsync(_noBodyText);
            return;
        }

        _logger.LogDebug("Sending anonymous message prompt.");
        var reply = await message.ReplyAsync(_anonymousReplyMessageText);
        await reply.AddReactionAsync(new Emoji(_yesEmoji));
        await reply.AddReactionAsync(new Emoji(_noEmoji));

        _pendingMessages.Add(reply.Id, message);
    }

    private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> reactionMessage, Cacheable<IMessageChannel, ulong> _, SocketReaction reaction)
    {
        if (!_pendingMessages.ContainsKey(reactionMessage.Id))
        {
            _logger.LogDebug("Reaction is not for a message sent by bot.");
            return;
        }

        var message = _pendingMessages[reactionMessage.Id];

        if (reaction.Emote.Name != _yesEmoji && reaction.Emote.Name != _noEmoji || reaction.UserId != message.Author.Id)
        {
            _logger.LogDebug("Reaction not valid, or not sent by the expected person.");
            return;
        }

        var isAnonymous = reaction.Emote.Name == "🇾";
        _logger.LogInformation($"Message is {(isAnonymous ? "being" : "not being")} sent anonymously.");
        var sender = $"{reaction.User.Value.Username}#{reaction.User.Value.Discriminator}";
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IOptions<DiscordConfiguration>>().Value;

            _logger.LogDebug("Saving message to database.");
            await context.Messages.AddAsync(new Message {
                Sender = sender,
                Body = message.Content,
                DateSent = DateTime.UtcNow,
                IsAnonymous = isAnonymous
            });
            await context.SaveChangesAsync();
            _logger.LogDebug("Message saved to database.");

            _logger.LogDebug("Sending message to admin channel.");

            var guild = _client.GetGuild(config.GuildId);
            var adminChannel = guild.GetTextChannel(config.AdminChannelId);
            var author = guild.GetUser(reaction.UserId);
            var adminMessage = isAnonymous
                ? $"Anonymous message received:\n>>> {message.Content}"
                : $"Message received from {author.Nickname ?? $"{author.Username}#{author.Discriminator}"}:\n>>> {message.Content}";

            await adminChannel.SendMessageAsync(adminMessage);
            _logger.LogDebug("Message sent to admin channel.");

            _logger.LogDebug("Notifying user that message was sent.");
            await (await reactionMessage.GetOrDownloadAsync()).ReplyAsync(_messageSentText);
            _pendingMessages.Remove(reactionMessage.Id);
            _logger.LogDebug("Handler complete.");
        }
        catch (HttpException exception)
        {
            _logger.LogError(exception, "An error has occurred.");
        }
    }
}
