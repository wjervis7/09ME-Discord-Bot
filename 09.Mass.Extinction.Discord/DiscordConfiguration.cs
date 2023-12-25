namespace Ninth.Mass.Extinction.Discord;

using Commands;

public class DiscordConfiguration
{
    public string ApiEndpoint { get; set; }
    public string Token { get; set; }
    public ulong GuildId { get; set; }
    public ulong AdminChannelId { get; set; }

    // ReSharper disable once CollectionNeverUpdated.Global
    public List<CommandConfiguration> CommandConfiguration { get; set; }
    public ulong EveryoneId { get; set; }
}
