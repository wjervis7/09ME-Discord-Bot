namespace _09.Mass.Extinction.Discord.Responses;

using System.Text.Json.Serialization;

[Serializable]
public class DiscordChannel
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
