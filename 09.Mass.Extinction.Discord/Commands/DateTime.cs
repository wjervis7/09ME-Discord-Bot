namespace Ninth.Mass.Extinction.Discord.Commands;

using Data;
using global::Discord;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class DateTime(ILogger<DateTime> logger, IServiceProvider serviceProvider) : ISlashCommand
{
    public SlashCommandOptionBuilder[] Options =>
        new[]
        {
            new SlashCommandOptionBuilder
            {
                Name = "year",
                Description = "The year for the date you want to have displayed.",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = true
            },
            new SlashCommandOptionBuilder
            {
                Name = "month",
                Description = "The month for the date you want to have displayed.",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = true
            },
            new SlashCommandOptionBuilder
            {
                Name = "day",
                Description = "The day for the date you want to have displayed.",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = true
            },
            new SlashCommandOptionBuilder
            {
                Name = "time",
                Description = "The time for the you want to have displayed.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true
            },
            new SlashCommandOptionBuilder
            {
                Name = "message",
                Description = "Message to display the formatted date/time with. Use $$ to indicate where the date/time should go.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = false
            }
        };

    public string Name => "date-time";
    public string Description => "Displays the provided datetime, in the Discord time format.";

    public List<ApplicationCommandPermission> Permissions { get; } = new();

    public async void Handle(SocketSlashCommand command)
    {
        logger.LogDebug("Entering command handler.");

        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        logger.LogDebug("Get user from database.");
        var user = await context.DiscordUsers.FindAsync(command.User.Id);

        if (string.IsNullOrWhiteSpace(user?.TimeZone))
        {
            logger.LogDebug("User does not have time zone set.");
            await command.RespondAsync("You must set your time zone, prior to using this command. Use `/time-zone set`, to set your time zone.", ephemeral: true);
            return;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
        var year = (int)command.Data.Options.GetValue<long>("year");
        var month = (int)command.Data.Options.GetValue<long>("month");
        var day = (int)command.Data.Options.GetValue<long>("day");
        var timeStr = command.Data.Options.GetValue<string>("time");
        var message = command.Data.Options.GetNullableValue<string?>("message");

        logger.LogDebug("Try parse datetime.");
        logger.LogInformation("Attempting to parse {dateTime}, in time zone {timeZone}.", $"Year: {year}, Month: {month}, Day: {day}, Time: {timeStr}", timeZone.Id);
        try
        {
            DateTimeHelper.ValidateDate(year, month, day);
            var (hour, minute) = DateTimeHelper.ParseTime(timeStr);
            var response = DateTimeHelper.DisplayDateTime(timeZone, year, month, day, hour, minute, message);
            await command.RespondAsync(response);
        }
        catch (ArgumentException e)
        {
            await command.RespondAsync(e.Message);
        }

        logger.LogDebug("Command handler complete.");
    }
}
