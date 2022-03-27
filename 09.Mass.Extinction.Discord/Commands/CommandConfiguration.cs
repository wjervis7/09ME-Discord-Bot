namespace _09.Mass.Extinction.Discord.Commands;

public class CommandConfiguration
{
    public string Command { get; set; }
    public CommandOptions Options { get; set; }
    public Dictionary<string, string> AdditionalSettings { get; set; }
}
