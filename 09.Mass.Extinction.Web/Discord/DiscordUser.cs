namespace _09.Mass.Extinction.Web.Discord;

using System.Text.Json.Serialization;

public class DiscordUser
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong Id { get; set; }

    [JsonPropertyName("username")] public string Username { get; set; }

    [JsonPropertyName("discriminator")] public string Discriminator { get; set; }

    [JsonPropertyName("avatar")] public string Avatar { get; set; }

    [JsonPropertyName("email")] public string Email { get; set; }
}
