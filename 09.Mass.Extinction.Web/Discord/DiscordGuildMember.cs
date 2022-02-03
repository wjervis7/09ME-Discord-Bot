namespace _09.Mass.Extinction.Web.Discord;

using System.Text.Json.Serialization;

public class DiscordGuildMember
{
    [JsonPropertyName("user")]
    public DiscordUser User { get; set; }

    [JsonPropertyName("nick")] 
    private string Nick { get; }

    [JsonIgnore]
    public string Nickname => string.IsNullOrWhiteSpace(Nick) ? User.Username : Nick;
}
