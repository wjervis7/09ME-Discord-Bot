namespace Ninth.Mass.Extinction.Discord.DiscordActivity;

using System.Collections.Concurrent;
using global::Discord;
using global::Discord.WebSocket;
using Microsoft.Extensions.Logging;

public class MessageCache(ILogger<MessageCache> logger, DiscordSocketClient client)
{
    private readonly ConcurrentDictionary<string, List<IMessage>> _messageCache = new();
    private readonly SemaphoreSlim _semaphore = new(1);

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
        logger.LogInformation("Checking if messages are in cache.");
        if (_messageCache.TryGetValue(key, out var messages))
        {
            logger.LogInformation("Returning messages from cache.");
            return messages;
        }

        logger.LogInformation("Cache did not contain messages, locking SemaphoreSlim.");
        await _semaphore.WaitAsync();
        try
        {
            logger.LogInformation("Checking cache again, after lock.");
            if (_messageCache.TryGetValue(key, out messages))
            {
                logger.LogInformation("Cache had messages, after lock. Returning messages from cache.");
                return messages;
            }

            logger.LogInformation("Getting messages from Discord.");
            return _messageCache[key] = await GetMessagesForChannel(channelId, dateToCheck);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<List<IMessage>> GetMessagesForChannel(ulong channelId, DateTimeOffset dateToCheck)
    {
        var channel = (SocketTextChannel)await client.GetChannelAsync(channelId);
        var fetchedMessages = await channel.GetMessagesAsync().FlattenAsync();

        var messages = new List<IMessage>(fetchedMessages);

        var atLimit = messages.Count == 100;
        var oldestMessage = messages.LastOrDefault();

        logger.LogInformation("Retrieved {messageCount} messages.", messages.Count);
        while (atLimit && oldestMessage?.CreatedAt >= dateToCheck)
        {
            logger.LogInformation("Oldest message was not before dateToCheck, so getting another batch.");
            fetchedMessages = (await channel.GetMessagesAsync(oldestMessage.Id, Direction.Before).FlattenAsync()).ToList();
            messages.AddRange(fetchedMessages);
            logger.LogInformation("Retrieved {messageCount} messages.", fetchedMessages.Count());
            atLimit = fetchedMessages.Count() == 100;
            oldestMessage = fetchedMessages.LastOrDefault();
        }

        logger.LogInformation("Finished fetching messages from Discord. Saving to cache, and returning to caller.");
        return messages;
    }
}
