namespace Ninth.Mass.Extinction.Discord.DiscordActivity;

public class UserChannelActivity
{
    public ulong Id { get; set; }
    public int PostCount { get; set; }
    public DateTimeOffset? LastPost { get; set; }
}
