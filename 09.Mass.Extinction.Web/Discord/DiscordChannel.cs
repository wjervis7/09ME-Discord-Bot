namespace _09.Mass.Extinction.Web.Discord;

using System.Text.Json.Serialization;

public class DiscordChannel
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
