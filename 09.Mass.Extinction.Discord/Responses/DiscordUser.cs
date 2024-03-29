﻿namespace Ninth.Mass.Extinction.Discord.Responses;

using System.Text.Json.Serialization;

[Serializable]
public class DiscordUser
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}
