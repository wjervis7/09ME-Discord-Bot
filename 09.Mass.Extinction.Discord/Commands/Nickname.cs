namespace _09.Mass.Extinction.Discord.Commands;

using System.Net;
using global::Discord;
using global::Discord.Net;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class Nickname : ISlashCommand
{
    private readonly ILogger<Nickname> _logger;

    public Nickname(ILogger<Nickname> logger, IOptionsMonitor<DiscordConfiguration> configurationMonitor)
    {
        _logger = logger;
        var configuration = configurationMonitor.CurrentValue;
        Permissions = ISlashCommand.SetPermissions(configuration, Name);
    }

    public string Name => "nickname";
    public string Description => "Sets nickname for a user.";
    public List<ApplicationCommandPermission> Permissions { get; }


    public SlashCommandOptionBuilder[] Options =>
        new[]
        {
            new SlashCommandOptionBuilder
            {
                Name = "user",
                Description = "The user whose nickname will be changed.",
                Type = ApplicationCommandOptionType.User,
                IsRequired = true
            },
            new SlashCommandOptionBuilder
            {
                Name = "name",
                Description = "The new nickname for the user. If not included, nickname will be cleared.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = false
            }
        };

    public async void Handle(SocketSlashCommand command)
    {
        var user = command.Data.Options.GetValue<SocketGuildUser>("user");
        var nickName = command.Data.Options.GetNullableValue<string>("name");

        if (nickName == null)
        {
            _logger.LogInformation("Clearing nickname for user: {user}.", user.Nickname);
        }
        else
        {
            _logger.LogInformation("Setting nickname for user: {user}, {nickname}.", user.Nickname ?? user.Username, nickName);
        }

        try
        {
            await user.ModifyAsync(properties => properties.Nickname = nickName);
            _logger.LogInformation("User nickname changed.");
        }
        catch (HttpException e)
        {
            _logger.LogError(e, "Unable to set nickname.");
            if (e.HttpCode == HttpStatusCode.Forbidden)
            {
                _logger.LogDebug("Forbidden, possibly because the bot's role is not higher than the target user.");
                await command.RespondAsync($"Unable to change nickname for {user.Nickname ?? user.Username}.  Make sure the bot's heirarchy is higher than the target user.");
                return;
            }
        }

        _logger.LogDebug("Notifying user that nickname changed.");
        await command.RespondAsync($"Nickname {(string.IsNullOrWhiteSpace(nickName) ? "cleared" : "changed")}.", ephemeral: true);
        _logger.LogDebug("Slash command complete.");
    }
}
