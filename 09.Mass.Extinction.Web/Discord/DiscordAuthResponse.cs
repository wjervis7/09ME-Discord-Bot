namespace _09.Mass.Extinction.Web.Services;

using System.Text.Json.Serialization;

public class DiscordAuthResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }
    [JsonPropertyName("expires_in")] public long Expiry { get; set; }
}
