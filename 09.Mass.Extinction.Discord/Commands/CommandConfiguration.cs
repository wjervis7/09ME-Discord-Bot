// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace _09.Mass.Extinction.Discord.Commands;

// ReSharper disable once ClassNeverInstantiated.Global
public class CommandConfiguration
{
    public string Command { get; set; }
    public CommandOptions Options { get; set; }

    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, string> AdditionalSettings { get; set; }
}
