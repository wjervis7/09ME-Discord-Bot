namespace _09.Mass.Extinction.Discord.DiscordActivity;

public class UserActivity
{
    public ulong UserId { get; init; }
    public string Nickname { get; init; }
    public List<UserChannelActivity> Activity { get; init; }
}
