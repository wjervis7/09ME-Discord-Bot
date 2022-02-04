namespace _09.Mass.Extinction.Web.Discord;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class DiscordService
{
    private readonly HttpClient _client;
    private readonly DiscordConfiguration _config;
    private readonly ILogger<DiscordService> _logger;

    public DiscordService(HttpClient client, IOptions<DiscordConfiguration> config, ILogger<DiscordService> logger)
    {
        _client = client;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<IEnumerable<DiscordGuildMember>> GetUsersByIds(IEnumerable<ulong> ids)
    {
        _logger.LogTrace("Getting users with ids: {ids}", ids);
        var users = new List<DiscordGuildMember>();

        var tasks = ids
            .Select(id => new HttpRequestMessage(HttpMethod.Get, $"guilds/{_config.GuildId}/members/{id}"))
            .Select(request => _client.SendAsync(request))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        _logger.LogTrace("Requests completed, processing requests.");
        foreach (var response in responses)
        {
            _logger.LogTrace("Processing response: '{request}'", response.RequestMessage?.RequestUri?.AbsoluteUri);
            if (!response.IsSuccessStatusCode)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting guild member: '{response}'", responseStr);
                continue;
            }

            try
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var discordUser = await JsonSerializer.DeserializeAsync<DiscordGuildMember>(responseStream);
                users.Add(discordUser);
                _logger.LogTrace("Finished processing response.");
            }
            catch (JsonException ex)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogError(ex, "Unable to deserialize the response: '{response}'", responseStr);
            }
        }

        return users;
    }

    public async Task<IEnumerable<DiscordGuildMember>> GetUsersByUsernames(IEnumerable<string> userNames)
    {
        _logger.LogTrace("Getting users with usernames: {usernames}", userNames);
        var users = new List<DiscordGuildMember>();

        var tasks = userNames
            .Select(userName => new HttpRequestMessage(HttpMethod.Get, $"guilds/{_config.GuildId}/members/search?query={userName.Split("#")[0]}"))
            .Select(request => _client.SendAsync(request))
            .ToList();

        var responses = await Task.WhenAll(tasks);
        
        _logger.LogTrace("Requests completed, processing requests.");
        foreach (var response in responses)
        {
            _logger.LogTrace("Processing response: '{request}'", response.RequestMessage?.RequestUri?.AbsoluteUri);
            if (!response.IsSuccessStatusCode)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting guild member: '{response}'", responseStr);
                continue;
            }

            try
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var discordUser = await JsonSerializer.DeserializeAsync<List<DiscordGuildMember>>(responseStream);
                if (discordUser?.Any() == true)
                {
                    users.Add(discordUser.First());
                }
                _logger.LogTrace("Finished processing response.");
            }
            catch (JsonException ex)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogError(ex, "Unable to deserialize the response: '{response}'", responseStr);
            }
        }

        return users;
    }

    public async Task<List<DiscordChannel>> GetChannels(IEnumerable<ulong> ids)
    {
        _logger.LogTrace("Getting channels with ids: {ids}", ids);
        var channels = new List<DiscordChannel>();

        var tasks = ids
            .Select(id => new HttpRequestMessage(HttpMethod.Get, $"channels/{id}"))
            .Select(request => _client.SendAsync(request))
            .ToList();

        var responses = await Task.WhenAll(tasks);
        
        _logger.LogTrace("Requests completed, processing requests.");
        foreach (var response in responses)
        {
            _logger.LogTrace("Processing response: '{request}'", response.RequestMessage?.RequestUri?.AbsoluteUri);
            if (!response.IsSuccessStatusCode)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting channel: '{response}'", responseStr);
                continue;
            }

            try
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var discordChannel = await JsonSerializer.DeserializeAsync<DiscordChannel>(responseStream);
                channels.Add(discordChannel);
                _logger.LogTrace("Finished processing response.");
            }
            catch (JsonException ex)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogError(ex, "Unable to deserialize the response: '{response}'", responseStr);
            }
        }

        return channels;
    }
}
