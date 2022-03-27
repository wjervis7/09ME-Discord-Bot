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

    private const string _messageSentText = "Message was sent to the admin team.";

    private readonly DiscordSocketClient _client;
    private readonly ILogger<PrivateMessageHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PrivateMessageHandler(ILogger<PrivateMessageHandler> logger, DiscordSocketClient client, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _client = client;
        _serviceProvider = serviceProvider;
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

        var sender = $"{message.Author.Username}#{message.Author.Discriminator}";
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IOptions<DiscordConfiguration>>().Value;

            _logger.LogDebug("Saving message to database.");
            await context.Messages.AddAsync(new Message {
                Sender = sender,
                Body = message.Content,
                DateSent = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            _logger.LogDebug("Message saved to database.");

            _logger.LogDebug("Sending message to admin channel.");
            var guild = _client.GetGuild(config.GuildId);
            var adminChannel = guild.GetTextChannel(config.AdminChannelId);
            var adminMessage = $"Message received from <@{message.Author.Id}>:\n>>> {message.Content}";

            await adminChannel.SendMessageAsync(adminMessage);
            _logger.LogDebug("Message sent to admin channel.");

            _logger.LogDebug("Notifying user that message was sent.");
            await message.ReplyAsync(_messageSentText);
            _logger.LogDebug("Handler complete.");
        }
        catch (HttpException exception)
        {
            _logger.LogError(exception, "An error has occurred.");
        }
    }
}
