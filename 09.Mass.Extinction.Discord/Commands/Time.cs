namespace _09.Mass.Extinction.Discord.Commands;

using Data;
using global::Discord;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Time : ISlashCommand
{
    private readonly ILogger<Time> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Time(ILogger<Time> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public SlashCommandOptionBuilder[] Options =>
        new[]
        {
            new SlashCommandOptionBuilder
            {
                Name = "time",
                Description = "The time you want to have displayed.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true
            }
        };

    public string Name => "time";
    public string Description => "Displays the provided time, in the Discord time format.";
    public List<ApplicationCommandPermission> Permissions { get; } = new();

    public async void Handle(SocketSlashCommand command)
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
            var (hour, minute) = DateTimeHelper.ParseTime(timeStr);
            var response = DateTimeHelper.DisplayTime(timeZone, hour, minute);
            await command.RespondAsync(response);
        }
        catch (ArgumentException e)
        {
            await command.RespondAsync(e.Message);
        }

        _logger.LogDebug("Command handler complete.");
    }
}
