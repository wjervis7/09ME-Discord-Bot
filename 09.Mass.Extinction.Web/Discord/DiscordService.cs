namespace _09.Mass.Extinction.Web.Discord;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class DiscordService
{
    private readonly HttpClient _client;
    private readonly DiscordConfiguration _config;

    public DiscordService(HttpClient client, IOptions<DiscordConfiguration> config)
    {
        _client = client;
        _config = config.Value;
    }

    public async Task<IEnumerable<DiscordGuildMember>> GetUsers(IEnumerable<ulong> ids)
    {
        var users = new List<DiscordGuildMember>();

        var tasks = ids
            .Select(id =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"guilds/{_config.GuildId}/members/{id}");
                request.Headers.Authorization = AuthenticationHeaderValue.Parse($"Bot {_config.Token}");
                return request;
            })
            .Select(request => _client.SendAsync(request))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            var responseStream = await response.Content.ReadAsStreamAsync();
            var discordUser = await JsonSerializer.DeserializeAsync<DiscordGuildMember>(responseStream);
            users.Add(discordUser);
        }

        return users;
    }

    public async Task<List<DiscordChannel>> GetChannels(IEnumerable<ulong> ids)
    {
        var channels = new List<DiscordChannel>();

        var tasks = ids
            .Select(id =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"channels/{id}");
                request.Headers.Authorization = AuthenticationHeaderValue.Parse($"Bot {_config.Token}");
                return request;
            })
            .Select(request => _client.SendAsync(request))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }
            var responseStream = await response.Content.ReadAsStreamAsync();
            var discordChannel = await JsonSerializer.DeserializeAsync<DiscordChannel>(responseStream);
            channels.Add(discordChannel);
        }

        return channels;
    }
}
