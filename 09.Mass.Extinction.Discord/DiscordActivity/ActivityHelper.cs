namespace _09.Mass.Extinction.Discord.DiscordActivity;

using System.Collections.Concurrent;
using System.Text;
using Commands;
using global::Discord;
using global::Discord.Net;
using global::Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ActivityHelper
{
    //private readonly Dictionary<ulong, List<IMessage>> _cachedMessages = new();
    private readonly DiscordSocketClient _client;
    private readonly DiscordConfiguration _config;

    private readonly ConcurrentDictionary<ulong, byte> _erroredChannels = new();
    private readonly ILogger<Activity> _logger;

    public ActivityHelper(ILogger<Activity> logger, IOptions<DiscordConfiguration> config, DiscordSocketClient client)
    {
        _logger = logger;
        _client = client;
        _config = config.Value;
    }

    public async Task<string> GetInactiveUsers(long posts, long days)
    {
        var dateToCheck = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(days));

        var guild = _client.GetGuild(_config.GuildId);
        await guild.DownloadUsersAsync();
        var guildMembers = guild.Users;

        var inactiveUsers = new ConcurrentDictionary<ulong, UserActivity>();

        await Parallel.ForEachAsync(guildMembers, async (user, _) =>
        {
            var userActivity = await GetUserActivity(user, dateToCheck);
            inactiveUsers.AddOrUpdate(user.Id, userActivity, (_, _) => userActivity);
        });

        var messageBuilder = new StringBuilder();

        if (inactiveUsers.Count == 0)
        {
            messageBuilder.AppendLine($"All users have made at least {posts} post(s), within the last {days} day(s).");
        }
        else
        {
            messageBuilder.AppendLine($"{inactiveUsers.Count} user(s) have not made {posts} post(s), within the last {days} day(s).");
            foreach (var inactiveUser in inactiveUsers.Values)
            {
                var totalPosts = inactiveUser.Activity.Sum(a => a.PostCount);
                if (totalPosts == 0)
                {
                    messageBuilder.AppendLine($"> {inactiveUser.User} has not made any posts, in the last {days} day(s).");
                }else if (totalPosts < posts)
                {
                    var latestPost = inactiveUser.Activity.Max(a => a.LastPost);
                    messageBuilder.AppendLine($"> {inactiveUser.User} has made {totalPosts} post(s), with the last post on <t:{latestPost!.Value.ToUnixTimeSeconds()}:f>.");
                }
            }
        }

        // ReSharper disable once InvertIf
        if (_erroredChannels.Any())
        {
            messageBuilder.AppendLine("I am unable to access the following channels/threads:");
            foreach (var erroredChannel in _erroredChannels.Values)
            {
                messageBuilder.AppendLine($"<#{erroredChannel}>");
            }
        }

        return messageBuilder.ToString();
    }

    public async Task<string> GetUserActivity(SocketGuildUser member, long days)
    {
        var dateToCheck = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(days));
        
        var userActivity = await GetUserActivity(member, dateToCheck);

        var messageBuilder = new StringBuilder();

        if (userActivity.Activity.Any())
        {
            messageBuilder.AppendLine($"{member.Nickname ?? member.Username}'s activity, over the last {days} day(s):");
            foreach (var activity in userActivity.Activity)
            {
                if (activity is UserThreadActivity threadActivity)
                {
                    messageBuilder.Append($"> In <#{threadActivity.Id}>(thread {threadActivity.ThreadName}, {(threadActivity.IsActive ? "active thread" : "archived thread")}): ");
                }
                else
                {
                    messageBuilder.Append($"> In <#{activity.Id}>: ");
                }

                messageBuilder.AppendLine($"{activity.PostCount} post(s), with the most recent one on <t:{activity.LastPost!.Value.ToUnixTimeSeconds()}:f>.");
            }
        }
        else
        {
            messageBuilder.AppendLine($"{member.Nickname ?? member.Username} has not made any posts, in the last {days} day(s).");
        }
        

        // ReSharper disable once InvertIf
        if (_erroredChannels.Any())
        {
            messageBuilder.AppendLine("I am unable to access the following channels/threads:");
            foreach (var erroredChannel in _erroredChannels)
            {
                messageBuilder.AppendLine($"<#{erroredChannel}>");
            }
        }

        return messageBuilder.ToString();
    }

    private async Task<UserActivity> GetUserActivity(IGuildUser member, DateTimeOffset dateToCheck)
    {
        var guild = _client.GetGuild(_config.GuildId);
        var channels = guild.TextChannels;

        var userActivity = new UserActivity {
            User = member.Nickname ?? member.Username,
            Activity = new List<UserChannelActivity>()
        };

        _logger.LogInformation("Getting activity for user {user}.", member.Nickname ?? member.Username);
        foreach (var channel in channels)
        {
            if (_erroredChannels.ContainsKey(channel.Id))
            {
                continue;
            }

            var activity = await GetUserActivityForChannel(member.Id, channel.Id, dateToCheck);

            if (activity == null)
            {
                continue;
            }

            userActivity.Activity.Add(activity);

            foreach (var thread in channel.Threads)
            {
                if (await GetUserActivityForChannel(member.Id, thread.Id, dateToCheck) is not UserThreadActivity threadActivity || threadActivity.PostCount == 0)
                {
                    continue;
                }

                threadActivity.ThreadName = thread.Name;
                threadActivity.IsActive = !thread.IsArchived;

                userActivity.Activity.Add(threadActivity);
            }
        }

        return userActivity;
    }

    private async Task<UserChannelActivity?> GetUserActivityForChannel(ulong userId, ulong channelId, DateTimeOffset dateToCheck)
    {
        if (_erroredChannels.ContainsKey(channelId))
        {
            _logger.LogInformation("Channel/thread has errored before; skipping.");
            return default;
        }

        try
        {
            var channel = (SocketTextChannel)await _client.GetChannelAsync(channelId);
            var fetchedMessages = await channel.GetMessagesAsync().FlattenAsync();

            var messages = new List<IMessage>(fetchedMessages);

            var atLimit = messages.Count == 100;
            var oldestMessage = messages.Last();

            while (atLimit && oldestMessage.CreatedAt >= dateToCheck)
            {
                fetchedMessages = (await channel.GetMessagesAsync(oldestMessage.Id, Direction.Before).FlattenAsync()).ToList();
                messages.AddRange(fetchedMessages);
                atLimit = fetchedMessages.Count() == 100;
                oldestMessage = fetchedMessages.Last();
            }

            var userMessages = messages.Where(m => m.Author.Id == userId && m.CreatedAt >= dateToCheck).ToList();

            if (!userMessages.Any())
            {
                return default;
            }

            var latestMessage = userMessages.FirstOrDefault()?.CreatedAt;

            var activity = channel.GetChannelType() == ChannelType.PublicThread ? new UserThreadActivity() : new UserChannelActivity();

            activity.Id = channelId;
            activity.LastPost = latestMessage;
            activity.PostCount = userMessages.Count;

            return activity;
        }
        catch (HttpException e)
        {
            _logger.LogError(e, "Unable to get activity for channel/thread {channel}.", channelId);
            _erroredChannels.AddOrUpdate(channelId, byte.MinValue, (_, _) => byte.MinValue);
            return default;
        }
    }
}
