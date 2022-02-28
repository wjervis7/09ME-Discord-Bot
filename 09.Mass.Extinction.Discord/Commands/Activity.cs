﻿namespace _09.Mass.Extinction.Discord.Commands;

using DiscordActivity;
using global::Discord;
using global::Discord.WebSocket;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Activity : ISlashCommand
{
    private const long _time = 14 * 60 * 1000 + 30 * 1000;
    private readonly ILogger<Activity> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Activity(ILogger<Activity> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public string Name => "activity";
    public string Description => "Gets user Discord activity.";

    public SlashCommandOptionBuilder[] Options =>
        new[] {
            new SlashCommandOptionBuilder {
                Name = "all",
                Description = "Gets all users, that haven't made x number of posts, in the last y days.",
                Type = ApplicationCommandOptionType.SubCommand,
                Options = new List<SlashCommandOptionBuilder> {
                    new() {
                        Name = "posts",
                        Description = "The minimum number of posts, that the user must make, to be considered active.",
                        Type = ApplicationCommandOptionType.Integer,
                        IsRequired = true
                    },
                    new() {
                        Name = "days",
                        Description = "The number of days, that the user must have posted within, to be considered active.",
                        Type = ApplicationCommandOptionType.Integer,
                        IsRequired = true
                    }
                }
            },
            new SlashCommandOptionBuilder {
                Name = "user",
                Description = "Gets how active a user is, within the last x days.",
                Type = ApplicationCommandOptionType.SubCommand,
                Options = new List<SlashCommandOptionBuilder> {
                    new() {
                        Name = "user",
                        Description = "The user to check activity for.",
                        Type = ApplicationCommandOptionType.User,
                        IsRequired = true
                    },
                    new() {
                        Name = "days",
                        Description = "The number of days, to check the user's activity.",
                        Type = ApplicationCommandOptionType.Integer,
                        IsRequired = true
                    }
                }
            }
        };

    public async Task Handle(SocketSlashCommand command)
    {
        await command.DeferAsync();
        await using var scope = _serviceProvider.CreateAsyncScope();
        var activityHelper = scope.ServiceProvider.GetRequiredService<ActivityHelper>();
        var timedOut = false;
        var timer = new Timer(_ => timedOut = true, null, _time, Timeout.Infinite);

        var subCommand = command.Data.Options.First();
        var response = subCommand.Name switch {
            "user" => await activityHelper.GetUserActivity(subCommand.Options.GetValue<SocketGuildUser>("user"), subCommand.Options.GetValue<long>("days")),
            "all" => await activityHelper.GetInactiveUsers(subCommand.Options.GetValue<long>("posts"), subCommand.Options.GetValue<long>("days")),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (timedOut)
        {
            _logger.LogDebug("Took over 15 minutes, so we can't use the interaction, to reply. Sending message to channel.");

            await SendInChunks($"<@{command.User.Id}>, the report has finished:\n{response}", command.Channel);
        }
        else
        {
            _logger.LogDebug("Stopping timer.");
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            _logger.LogDebug("Using interaction to reply.");
            await command.ModifyOriginalResponseAsync(properties => properties.Content = response);
        }

        _logger.LogDebug("Command handler complete.");
    }

    private static async Task SendInChunks(string message, ISocketMessageChannel channel)
    {
        var chunks = new List<string>();
        const int chunkSize = 200;

        while (message.Length >= chunkSize)
        {
            var chunk = message[..chunkSize];
            var i = chunk.LastIndexOf("\n", StringComparison.Ordinal);
            chunks.Add(chunk[..i]);
            message = message[i..];
        }

        foreach (var chunk in chunks)
        {
            await channel.SendMessageAsync(chunk);
        }
    }
}