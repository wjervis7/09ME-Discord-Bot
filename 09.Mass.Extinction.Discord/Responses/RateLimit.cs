// ReSharper disable UnusedMember.Global

namespace Ninth.Mass.Extinction.Discord.Responses;

using System.Text.Json.Serialization;

[Serializable]
public class RateLimit
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("retry_after")]
    public int RetryAfter { get; set; }

    [JsonPropertyName("global")]
    public bool IsGlobal { get; set; }
}
