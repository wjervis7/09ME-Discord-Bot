namespace _09.Mass.Extinction.Discord;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Responses;

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

    public async Task<IEnumerable<DiscordGuildMember>> GetUsersByIds(IList<ulong> ids)
    {
        if (ids.Any() != true)
        {
            throw new ArgumentException("Cannot be empty", nameof(ids));
        }
        
        _logger.LogTrace("Getting users with ids: {ids}", ids);
        var users = new List<DiscordGuildMember>();

        var discordUsers = (await GetFromDiscord<DiscordGuildMember>(new HashSet<string>(ids.Select(id=> $"guilds/{_config.GuildId}/members/{id}")))).ToList();

        if (!discordUsers.Any())
        {
            return users;
        }

        users.AddRange(discordUsers);
        return users;
    }

    public async Task<IEnumerable<DiscordGuildMember>> GetUsersByUsernames(IList<string> userNames)
    {
        if (userNames.Any() != true)
        {
            throw new ArgumentException("Cannot be empty", nameof(userNames));
        }
        
        _logger.LogTrace("Getting users with usernames: {usernames}", userNames);
        var users = new List<DiscordGuildMember>();

        var discordUsers = (await GetFromDiscord<List<DiscordGuildMember>>(new HashSet<string>(userNames.Select(userName => $"guilds/{_config.GuildId}/members/search?query={userName.Split("#")[0]}")))).ToList();

        if (!discordUsers.Any())
        {
            return users;
        }

        users.AddRange(discordUsers.Where(list => list.Any()).SelectMany(list => list.ToList()));
        return users;
    }

    public async Task<List<DiscordChannel>> GetChannels(IList<ulong> ids)
    {
        if (ids.Any() != true)
        {
            throw new ArgumentException("Cannot be empty", nameof(ids));
        }

        _logger.LogTrace("Getting channels with ids: {ids}", ids);
        var channels = new List<DiscordChannel>();

        var discordChannels = (await GetFromDiscord<DiscordChannel>(new HashSet<string>(ids.Select(id => $"channels/{id}")))).ToList();

        if (!discordChannels.Any())
        {
            return channels;
        }

        channels.AddRange(discordChannels);
        return channels;
    }

    private async Task<IEnumerable<TResponse>> GetFromDiscord<TResponse>(HashSet<string> urls) where TResponse : new()
    {
        var responseObjects = new List<TResponse>();

        _logger.LogTrace("Creating and sending requests.");
        var requests = urls
            .Select(GetFromDiscord)                     
            .ToList();
        _logger.LogTrace("Requests created and sent.");

        _logger.LogTrace("Waiting for requests to complete.");
        var responses = await Task.WhenAll(requests);
        _logger.LogTrace("Requests complete.  Processing responses.");

        foreach (var response in responses)
        {
            _logger.LogTrace("Processing response for request {request}.", response.RequestMessage?.RequestUri);

            if (!response.IsSuccessStatusCode)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error with request {request}: '{response}'", response.RequestMessage?.RequestUri, responseStr);
                continue;
            }

            try
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseObj = await JsonSerializer.DeserializeAsync<TResponse>(responseStream);
                if (responseObj != null)
                {
                    responseObjects.Add(responseObj);
                }

                _logger.LogTrace("Finished processing response.");
            }
            catch (JsonException ex)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogError(ex, "Unable to deserialize the response, for requst '{request}': '{response}'", response.RequestMessage?.RequestUri, responseStr);
            }
        }

        return responseObjects;
    }

    private async Task<HttpResponseMessage> GetFromDiscord(string url)
    {
        _logger.LogTrace("Creating request: '{request}'", url);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        HttpResponseMessage response;

        do
        {
            _logger.LogTrace("Sending request: '{request}'", url);
            response = await _client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.TooManyRequests)
            {
                break;
            }

            var responseStream = await response.Content.ReadAsStreamAsync();
            var rateLimit = await JsonSerializer.DeserializeAsync<RateLimit>(responseStream);
            _logger.LogTrace($"Being rate limited.  Waiting {rateLimit!.RetryAfter} seconds, before trying again.");
            Thread.Sleep(rateLimit.RetryAfter);
        } while (response.StatusCode != HttpStatusCode.TooManyRequests);

        _logger.LogTrace("Request completed.");

        return response;
    }
}
