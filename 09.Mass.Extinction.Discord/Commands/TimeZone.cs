namespace Ninth.Mass.Extinction.Discord.Commands;

using Data;
using Data.Entities;
using global::Discord;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class TimeZone(ILogger<TimeZone> logger, IServiceProvider serviceProvider) : ISlashCommand
{
    public string Name => "time-zone";
    public string Description => "Set or remove your timezone.";
    public List<ApplicationCommandPermission> Permissions { get; } = new();

    public SlashCommandOptionBuilder[] Options =>
        new[]
        {
            new SlashCommandOptionBuilder
            {
                Name = "set",
                Description = "Sets your time zone.",
                Type = ApplicationCommandOptionType.SubCommand,
                Options = new List<SlashCommandOptionBuilder>
                {
                    new()
                    {
                        Name = "zone",
                        Description = "The name of your time zone.",
                        Type = ApplicationCommandOptionType.String,
                        IsRequired = true
                    }
                }
            },
            new SlashCommandOptionBuilder
            {
                Name = "remove",
                Description = "Removes your time zone.",
                Type = ApplicationCommandOptionType.SubCommand
            }
        };

    public async void Handle(SocketSlashCommand command)
    {
        logger.LogDebug("Entering command handler.");
        await using var scope = serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var subCommand = command.Data.Options.First();
        var response = subCommand.Name switch
        {
            "set" => await SetTimeZone(context, command.User.Id, subCommand.Options.GetValue<string>("zone")),
            "remove" => await RemoveTimeZone(context, command.User.Id),
            _ => throw new InvalidOperationException("Invalid command")
        };

        logger.LogDebug("Time zone set/removed.");
        await command.RespondAsync(response, ephemeral: true);

        logger.LogDebug("Command handler complete.");
    }

    private async Task<string> SetTimeZone(ApplicationDbContext context, ulong userId, string timeZone)
    {
        logger.LogDebug("Executing Set Time Zone subcommand.");

        logger.LogDebug("Checking if time zone is valid.");

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            timeZone = tz.Id;
        }
        catch (TimeZoneNotFoundException)
        {
            logger.LogDebug("Time zone not found.");
            return "Unable to find your time zone. Please try again. You can use the IANA database, to find your time zone.";
        }

        logger.LogInformation("Getting user's time zone from the database.");
        var user = await context.DiscordUsers.FindAsync(userId);
        if (user == null)
        {
            logger.LogInformation("User not found; creating user.");
            user = context.DiscordUsers.Add(new DiscordUser
            {
                Id = userId
            }).Entity;
        }

        logger.LogInformation("Setting user's time zone.");
        user.TimeZone = timeZone;
        logger.LogDebug("Saving database changes.");
        await context.SaveChangesAsync();
        logger.LogDebug("Database changes complete.");

        logger.LogInformation("Finished setting user's timezone.");
        return "Your time zone has been saved.";
    }

    private async Task<string> RemoveTimeZone(ApplicationDbContext context, ulong userId)
    {
        logger.LogDebug("Executing Remove Time Zone subcommand.");
        logger.LogInformation("Getting user's time zone from the database.");
        var user = await context.DiscordUsers.FindAsync(userId);
        if (user == null)
        {
            logger.LogDebug("User not found.");
            return "Your time zone was never set; nothing to remove.";
        }

        logger.LogInformation("Clearing user's time zone.");
        user.TimeZone = "";
        logger.LogDebug("Saving database changes.");
        await context.SaveChangesAsync();
        logger.LogDebug("Database changes complete.");

        logger.LogInformation("Finished clearing user's timezone.");
        return "Your time zone has been removed.";
    }
}
