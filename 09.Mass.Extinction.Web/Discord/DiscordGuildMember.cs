namespace _09.Mass.Extinction.Web.Discord;

using System.Text.Json.Serialization;

public class DiscordGuildMember
{
    private string _nickname;

    [JsonPropertyName("user")] 
    public DiscordUser User { get; set; }

    [JsonPropertyName("nick")]
    public string Nickname {
        get => string.IsNullOrWhiteSpace(_nickname) ? User.Username : _nickname;
        set => _nickname = value;
    }
}
