﻿namespace _09.Mass.Extinction.Discord.Commands;

using global::Discord;
using global::Discord.WebSocket;

public interface ISlashCommand
{
    string Name { get; }
    string Description { get; }
    public SlashCommandOptionBuilder[] Options { get; }
    Task Handle(SocketSlashCommand command);
    static bool IsInvalidUsage(CommandConfiguration? commandConfiguration, SocketInteraction command, out string message)
    {
        message = "";

        if (commandConfiguration == null)
        {
            return false;
        }

        var options = commandConfiguration.Options;
        var guild = DiscordClient.Guild;
        var channelThreads = new List<ulong>();
        var roleUsers = new List<ulong>();
        foreach (var channel in options.AllowedChannels.Select(channelId => guild.GetTextChannel(channelId)))
        {
            channelThreads.AddRange(channel.Threads.Select(t => t.Id));
        }
        foreach (var role in options.AllowedRoles.Select(roleId => guild.GetRole(roleId)))
        {
            roleUsers.AddRange(role.Members.Select(m => m.Id));
        }

        var inValidChannel = channelThreads.Count != 0 && channelThreads.Any(t => t == command.Channel.Id) ||
                             options.AllowedChannels.Count == 0 ||
                             options.AllowedChannels.Any(c => c == command.Channel.Id);
        var isValidUser = roleUsers.Count != 0 && roleUsers.Any(u => u == command.User.Id) ||
                          options.AllowedUsers.Count == 0 ||
                          options.AllowedUsers.Any(u => u == command.User.Id);

        if (!inValidChannel)
        {
            message = "This command cannot be used in this channel.";
            return true;
        }

        if (isValidUser)
        {
            return false;
        }

        message = "You are not authorized to use this command.";
        return true;
    }
}