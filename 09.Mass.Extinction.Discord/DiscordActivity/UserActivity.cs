namespace _09.Mass.Extinction.Discord.DiscordActivity;

public class UserActivity
{
    public ulong UserId { get; set; }
    public string Nickname { get; set; }
    public List<UserChannelActivity> Activity { get; set; }
}
