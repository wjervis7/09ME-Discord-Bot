namespace _09.Mass.Extinction.Discord.Commands;

using Data;
using global::Discord;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemDateTime = System.DateTime;

public class DateTime : ISlashCommand
{
    private const string invalidYear = "The year you entered, is not valid.";
    private const string invalidMonth = "The month you entered, is not valid.";
    private const string invalidDay = "The day you entered, is not valid.";

    private readonly ILogger<DateTime> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DateTime(ILogger<DateTime> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public SlashCommandOptionBuilder[] Options =>
        new[] {
            new SlashCommandOptionBuilder {
                Name = "year",
                Description = "The year for the date you want to have displayed.",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = true
            },
            new SlashCommandOptionBuilder {
                Name = "month",
                Description = "The month for the date you want to have displayed.",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = true
            },
            new SlashCommandOptionBuilder {
                Name = "day",
                Description = "The day for the date you want to have displayed.",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = true
            },
            new SlashCommandOptionBuilder {
                Name = "time",
                Description = "The time for the you want to have displayed.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true
            }
        };

    public string Name => "date-time";
    public string Description => "Displays the provided datetime, in the Discord time format.";

    public List<ApplicationCommandPermission> Permissions { get; } = new ();

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
            await command.RespondAsync("You must set your time zone, prior to using this command. Use `/time-zone set`, to set your time zone.", ephemeral: true);
            return;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
        var year = (int)command.Data.Options.GetValue<long>("year");
        var month = (int)command.Data.Options.GetValue<long>("month");
        var day = (int)command.Data.Options.GetValue<long>("day");
        var timeStr = command.Data.Options.GetValue<string>("time");

        _logger.LogDebug("Try parse datetime.");
        _logger.LogInformation("Attempting to parse {dateTime}, in time zone {timeZone}.", $"Year: {year}, Month: {month}, Day: {day}, Time: {timeStr}", timeZone.Id);
        try
        {
            ValidateDate(year, month, day);
            var (hour, minute) = Time.ParseTime(timeStr);
            var response = DisplayDateTime(timeZone, year, month, day, hour, minute);
            await command.RespondAsync(response);
        }
        catch (ArgumentException e)
        {
            await command.RespondAsync(e.Message);
        }

        _logger.LogDebug("Command handler complete.");
    }

    private static void ValidateDate(int year, int month, int day)
    {
        
        if (year < 1)
        {
            throw new ArgumentException(invalidYear);
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentException(invalidMonth);
        }

        switch (month)
        {
            case 1:
            case 3:
            case 5:
            case 7:
            case 8:
            case 10:
            case 12:
                if (day is < 1 or > 31)
                {
                    throw new ArgumentException(invalidDay);
                }

                break;
            case 2:
                if (day < 1 || 
                    year % 4 == 0 && day > 29 || // leap years
                    year % 4 != 0 && day > 28)
                {
                    throw new ArgumentException(invalidDay);
                }

                break;
            case 4:
            case 6:
            case 9:
            case 11:
                if (day is < 1 or > 30)
                {
                    throw new ArgumentException(invalidDay);
                }

                break;
        }
    }

    private static string DisplayDateTime(TimeZoneInfo userTimeZone, int year, int month, int day, int hour, int minute)
    {
        Console.WriteLine("Year: {0}, Month: {1}, Day: {2}, Hour: {3}, Minute: {4}", year, month, day, hour, minute);
        var date = new SystemDateTime(year, month, day);
        var time = new DateTimeOffset(year, month, day, hour, minute, 0, 0, userTimeZone.GetUtcOffset(date));

        return $"You entered: <t:{time.ToUnixTimeSeconds()}:f>";
    }
}
