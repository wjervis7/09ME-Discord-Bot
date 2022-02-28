namespace _09.Mass.Extinction.Discord.Commands;

using Data;
using global::Discord;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Time : ISlashCommand
{
    private const string invalidTimeFormat = "The time you entered, is not a valid format. Use either 12 hour or 24 hour format.";

    private readonly ILogger<Time> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Time(ILogger<Time> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public SlashCommandOptionBuilder[] Options =>
        new[] {
            new SlashCommandOptionBuilder {
                Name = "time",
                Description = "The time you want to have displayed.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true
            }
        };

    public string Name => "time";
    public string Description => "Displays the provided time, in the Discord time format.";

    public async Task Handle(SocketSlashCommand command)
    {
        _logger.LogDebug("Entering command handler.");

        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _logger.LogDebug("Get user from database.");
        var user = await context.DiscordUsers.FindAsync(command.User.Id);

        if (string.IsNullOrWhiteSpace(user?.TimeZone))
        {
            _logger.LogDebug("User does not have time zone set.");
            await command.RespondAsync("You must set your time zone, prior to using this command. Use `/time-zone set`, to set your time zone.");
            return;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
        var timeStr = command.Data.Options.GetValue<string>("time");

        _logger.LogDebug("Try parse datetime.");
        _logger.LogInformation("Attempting to parse {time}, in time zone {timeZone}.", timeStr, timeZone.Id);
        try
        {
            var (hour, minute) = ParseTime(timeStr);
            var response = DisplayTime(timeZone, hour, minute);
            await command.RespondAsync(response);
        }
        catch (ArgumentException e)
        {
            await command.RespondAsync(e.Message);
        }

        _logger.LogDebug("Command handler complete.");
    }

    public static (int hour, int minute) ParseTime(string timeStr)
    {
        var timeParts = timeStr.Split(new[]{':', '.', 'h', ' '}, StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).ToArray();
        string? period = null;

        var firstPart = timeParts.ElementAtOrDefault(0);

        if (!int.TryParse(firstPart, out var hour))
        {

            if (firstPart?.Length > 2)
            {
                if (!int.TryParse(firstPart[..2], out hour))
                {
                    hour = int.Parse(firstPart[..1]);
                    period = firstPart[1..];
                }
                else
                {
                    period = firstPart[2..];
                }
            }
            else
            {
                throw new ArgumentException(invalidTimeFormat);
            }
        }

        if (hour > 23)
        {
            throw new ArgumentException(invalidTimeFormat);
        }

        var secondPart = timeParts.ElementAtOrDefault(1);

        if (!int.TryParse(secondPart, out var minute))
        {
            if (secondPart?.Length > 2)
            {
                minute = int.Parse(secondPart[..2]);
                period ??= secondPart[2..];
            }
            else
            {
                minute = 0;
                period ??= timeParts.ElementAtOrDefault(1);
            }
        }
        else
        {
            period ??= timeParts.ElementAtOrDefault(2);
        }

        if (minute > 59)
        {
            throw new ArgumentException(invalidTimeFormat);
        }

        if (string.IsNullOrWhiteSpace(period))
        {
            return (hour, minute);
        }

        switch (period.ToLower())
        {
            case "am":
                break;
            case "pm":
                hour += 12;
                break;
            default:
                throw new ArgumentException(invalidTimeFormat);
        }

        return (hour, minute);
    }

    private static string DisplayTime(TimeZoneInfo userTimeZone, int hour, int minute)
    {
        var now = DateTimeOffset.Now;


        var time = new DateTimeOffset(now.Year, now.Month, now.Day, hour, minute, 0, 0, userTimeZone.BaseUtcOffset);

        return $"You entered: <t:{time.ToUnixTimeSeconds()}:t>";
    }
}
