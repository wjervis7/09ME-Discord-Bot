namespace _09.Mass.Extinction.Discord.Commands;

public class CommandOptions
{
    public List<ulong> AllowedRoles { get; } = new();
    public List<ulong> AllowedUsers { get; } = new();
    public List<ulong> AllowedChannels { get; } = new();
}
