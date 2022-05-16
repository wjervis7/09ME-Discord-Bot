﻿// ReSharper disable CollectionNeverUpdated.Global

namespace _09.Mass.Extinction.Discord.Commands;

// ReSharper disable once ClassNeverInstantiated.Global
public class CommandOptions
{
    public List<ulong> AllowedRoles { get; } = new();
    public List<ulong> AllowedUsers { get; } = new();
    public List<ulong> AllowedChannels { get; } = new();
}
