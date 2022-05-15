namespace _09.Mass.Extinction.Discord.DiscordActivity;

using System.Collections.Concurrent;
using global::Discord;
using global::Discord.WebSocket;
using Microsoft.Extensions.Logging;

public class MessageCache
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger<MessageCache> _logger;
    private readonly ConcurrentDictionary<string, List<IMessage>> _messageCache = new();
    private readonly SemaphoreSlim _semaphore = new(1);

    public MessageCache(ILogger<MessageCache> logger, DiscordSocketClient client)
    {
        _logger = logger;
        _client = client;
    }

    public void ClearCache(DateTimeOffset dateToCheck)
    {
        var keys = _messageCache.Keys.Where(s => s.EndsWith(dateToCheck.ToUnixTimeSeconds().ToString()));
        foreach (var key in keys)
        {
            _messageCache.Remove(key, out _);
        }
    }

    public async Task<List<IMessage>> GetMessages(ulong channelId, DateTimeOffset dateToCheck)
    {
        var key = $"{channelId}-{dateToCheck.ToUnixTimeSeconds()}";
        _logger.LogInformation("Checking if messages are in cache.");
        if (_messageCache.TryGetValue(key, out var messages))
        {
            _logger.LogInformation("Returning messages from cache.");
            return messages;
        }

        _logger.LogInformation("Cache did not contain messages, locking SemaphoreSlim.");
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("Checking cache again, after lock.");
            if (_messageCache.TryGetValue(key, out messages))
            {
                _logger.LogInformation("Cache had messages, after lock. Returning messages from cache.");
                return messages;
            }

            _logger.LogInformation("Getting messages from Discord.");
            return _messageCache[key] = await GetMessagesForChannel(channelId, dateToCheck);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<List<IMessage>> GetMessagesForChannel(ulong channelId, DateTimeOffset dateToCheck)
    {
        var channel = (SocketTextChannel)await _client.GetChannelAsync(channelId);
        var fetchedMessages = await channel.GetMessagesAsync().FlattenAsync();

        var messages = new List<IMessage>(fetchedMessages);

        var atLimit = messages.Count == 100;
        var oldestMessage = messages.LastOrDefault();

        _logger.LogInformation("Retrieved {messageCount} messages.", messages.Count);
        while (atLimit && oldestMessage?.CreatedAt >= dateToCheck)
        {
            _logger.LogInformation("Oldest message was not before dateToCheck, so getting another batch.");
            fetchedMessages = (await channel.GetMessagesAsync(oldestMessage.Id, Direction.Before).FlattenAsync()).ToList();
            messages.AddRange(fetchedMessages);
            _logger.LogInformation("Retrieved {messageCount} messages.", fetchedMessages.Count());
            atLimit = fetchedMessages.Count() == 100;
            oldestMessage = fetchedMessages.LastOrDefault();
        }

        _logger.LogInformation("Finished fetching messages from Discord. Saving to cache, and returning to caller.");
        return messages;
    }
}
