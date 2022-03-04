namespace _09.Mass.Extinction.Discord.Commands;

using Data;
using Data.Entities;
using global::Discord;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class TimeZone : ISlashCommand
{
    private readonly ILogger<TimeZone> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TimeZone(ILogger<TimeZone> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public string Name => "time-zone";
    public string Description => "Set or remove your timezone.";

    public SlashCommandOptionBuilder[] Options =>
        new[] {
            new SlashCommandOptionBuilder {
                Name = "set",
                Description = "Sets your time zone.",
                Type = ApplicationCommandOptionType.SubCommand,
                Options = new List<SlashCommandOptionBuilder> {
                    new() {
                        Name = "zone",
                        Description = "The name of your time zone.",
                        Type = ApplicationCommandOptionType.String,
                        IsRequired = true
                    }
                }
            },
            new SlashCommandOptionBuilder {
                Name = "remove",
                Description = "Removes your time zone.",
                Type = ApplicationCommandOptionType.SubCommand
            }
        };

    public async Task Handle(SocketSlashCommand command)
    {
        _logger.LogDebug("Entering command handler.");
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var subCommand = command.Data.Options.First();
        var response = subCommand.Name switch {
            "set" => await SetTimeZone(context, command.User.Id, subCommand.Options.GetValue<string>("zone")),
            "remove" => await RemoveTimeZone(context, command.User.Id),
            _ => throw new ArgumentOutOfRangeException()
        };

        _logger.LogDebug("Time zone set/removed.");
        await command.RespondAsync(response, ephemeral: true);

        _logger.LogDebug("Command handler complete.");
    }

    private async Task<string> SetTimeZone(ApplicationDbContext context, ulong userId, string timeZone)
    {
        _logger.LogDebug("Executing Set Time Zone subcommand.");

        _logger.LogDebug("Checking if time zone is valid.");

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            timeZone = tz.Id;
        }
        catch (TimeZoneNotFoundException)
        {
            _logger.LogDebug("Time zone not found.");
            return "Unable to find your time zone. Please try again. You can use the IANA database, to find your time zone.";
        }

        _logger.LogInformation("Getting user's time zone from the database.");
        var user = await context.DiscordUsers.FindAsync(userId);
        if (user == null)
        {
            _logger.LogInformation("User not found; creating user.");
            user = new DiscordUser {
                Id = userId
            };
        }

        _logger.LogInformation("Setting user's time zone.");
        user.TimeZone = timeZone;
        _logger.LogDebug("Saving database changes.");
        await context.SaveChangesAsync();
        _logger.LogDebug("Database changes complete.");

        _logger.LogInformation("Finished setting user's timezone.");
        return "Your time zone has been saved.";
    }

    private async Task<string> RemoveTimeZone(ApplicationDbContext context, ulong userId)
    {
        _logger.LogDebug("Executing Remove Time Zone subcommand.");
        _logger.LogInformation("Getting user's time zone from the database.");
        var user = await context.DiscordUsers.FindAsync(userId);
        if (user == null)
        {
            _logger.LogDebug("User not found.");
            return "Your time zone was never set; nothing to remove.";
        }

        _logger.LogInformation("Clearing user's time zone.");
        user.TimeZone = "";
        _logger.LogDebug("Saving database changes.");
        await context.SaveChangesAsync();
        _logger.LogDebug("Database changes complete.");

        _logger.LogInformation("Finished clearing user's timezone.");
        return "Your time zone has been removed.";
    }
}
