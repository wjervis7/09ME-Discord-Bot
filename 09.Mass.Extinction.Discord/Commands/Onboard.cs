namespace _09.Mass.Extinction.Discord.Commands;

using global::Discord;
using global::Discord.Net;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class Onboard : ISlashCommand
{
    private readonly IOptionsMonitor<DiscordConfiguration> _config;
    private readonly ILogger<Onboard> _logger;

    public Onboard(ILogger<Onboard> logger, IOptionsMonitor<DiscordConfiguration> configurationMonitor)
    {
        _logger = logger;
        _config = configurationMonitor;
        Permissions = ISlashCommand.SetPermissions(_config.CurrentValue, Name);
    }

    public string Name => "onboard";
    public string Description => "Onboards a user. Sets nickname, and assigns default roles.";

    public SlashCommandOptionBuilder[] Options =>
        new[]
        {
            new SlashCommandOptionBuilder
            {
                Name = "user",
                Description = "The user to be onboarded.",
                Type = ApplicationCommandOptionType.User,
                IsRequired = true
            },
            new SlashCommandOptionBuilder
            {
                Name = "name",
                Description = "The nickname, for the user. Optional.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = false
            },
            new SlashCommandOptionBuilder
            {
                Name = "assign-roles",
                Description = "If true, assigns the Ninth Mass Extinction role.",
                Type = ApplicationCommandOptionType.Boolean,
                IsRequired = false
            },
            new SlashCommandOptionBuilder
            {
                Name = "optional-role",
                Description = "Optional role to assign the user.",
                Type = ApplicationCommandOptionType.Role,
                IsRequired = false
            }
        };

    public List<ApplicationCommandPermission> Permissions { get; }

    public async void Handle(SocketSlashCommand command)
    {
        _logger.LogInformation("Starting onboard process, for user.");

        var user = command.Data.Options.GetValue<SocketGuildUser>("user");
        var nickName = command.Data.Options.GetNullableValue<string>("name");
        var assignRoles = command.Data.Options.GetNullableValue<bool?>("assign-roles");
        var role = command.Data.Options.GetNullableValue<SocketRole>("optional-role");

        var message = "";

        if (string.IsNullOrWhiteSpace(nickName))
        {
            _logger.LogInformation("Nickname was not provided. Not changing nickname.");
        }
        else
        {
            _logger.LogInformation("Changing {user}'s nickname to {nickname}.", user.Id, nickName);
            try
            {
                var oldNickname = user.Nickname;
                await user.ModifyAsync(properties => properties.Nickname = nickName);
                _logger.LogInformation("Nickname changed.");
                message += $"\nNickname changed from {oldNickname} to {nickName}.";
            }
            catch (HttpException e)
            {
                _logger.LogWarning(e, "Unable to change user's nickname.");
                message += "\nCould not change the nickname.";
            }
        }

        if (assignRoles != true)
        {
            _logger.LogInformation("Flag to assign default role was not passed, or was false. Not assigning default role.");
        }
        else
        {
            var commandConfig = _config.CurrentValue.CommandConfiguration.SingleOrDefault(cc => cc.Command == Name);
            var defaultRoleStr = commandConfig?.AdditionalSettings["DefaultRole"] ?? "";
            var defaultRole = ulong.Parse(defaultRoleStr);
            _logger.LogInformation("Adding role {role}, to {user}.", defaultRole, user.Id);
            try
            {
                await user.AddRoleAsync(defaultRole);
                _logger.LogInformation("Role added.");
                message += $"\nUser added to role <@&{defaultRole}>.";
            }
            catch (HttpException e)
            {
                _logger.LogWarning(e, "Unable to add user to role.");
                message += $"\nCould not add user to role <@&{defaultRole}>.";
            }
        }

        if (role == null)
        {
            _logger.LogInformation("No role was passed, to assign to the user.");
        }
        else
        {
            _logger.LogInformation("Adding role {role}, to {user}.", role.Id, user.Id);
            try
            {
                await user.AddRoleAsync(role);
                _logger.LogInformation("Role added.");
                message += $"\nUser added to role <@&{role.Id}>.";
            }
            catch (HttpException e)
            {
                _logger.LogWarning(e, "Unable to add role to user.");
                message += $"\nCould not add user to role <@&{role.Id}>.";
            }
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            message = "No changes were made...";
            _logger.LogInformation("Nothing changed...");
        }
        else
        {
            _logger.LogInformation("Finished updating user.");
        }

        await command.RespondAsync(message.Trim('\n'));
    }
}
