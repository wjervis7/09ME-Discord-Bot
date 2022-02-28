namespace _09.Mass.Extinction.Discord.Responses;

using System.Text.Json.Serialization;

[Serializable]
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
