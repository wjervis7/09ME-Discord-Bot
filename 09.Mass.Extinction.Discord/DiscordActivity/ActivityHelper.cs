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
    private readonly ILogger<Activity> _logger;
    private readonly DiscordSocketClient _client;
    private readonly DiscordConfiguration _config;
    private readonly MessageCache _cache;

    private readonly ConcurrentDictionary<ulong, byte> _erroredChannels = new();
    private readonly List<ulong> _ignoredChannels;
    private readonly List<ulong> _ignoredUsers;
    private readonly List<ulong> _ignoredRoles;

    public ActivityHelper(ILogger<Activity> logger, IOptions<DiscordConfiguration> config, DiscordSocketClient client, MessageCache cache)
    {
        _logger = logger;
        _client = client;
        _cache = cache;
        _config = config.Value;

        var commandConfig = _config.CommandConfiguration.SingleOrDefault(cc => cc.Command == "activity");
        var ignoredChannelsStr = commandConfig?.AdditionalSettings["ExcludedChannels"] ?? "";
        _ignoredChannels = ignoredChannelsStr.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(ulong.Parse).ToList();
        var ignoredUsersStr = commandConfig?.AdditionalSettings["ExcludedUsers"] ?? "";
        _ignoredUsers = ignoredUsersStr.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(ulong.Parse).ToList();
        var ignoredRolesStr = commandConfig?.AdditionalSettings["ExcludedRoles"] ?? "";
        _ignoredRoles = ignoredRolesStr.Split(",", StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(ulong.Parse).ToList();
    }

    public async Task<string> GetInactiveUsers(long posts, long days)
    {
        var dateToCheck = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(days));

        var guild = _client.GetGuild(_config.GuildId);
        await guild.DownloadUsersAsync();
        var guildMembers = guild.Users.Where(m =>
        {
            if (_ignoredUsers.Contains(m.Id))
            {
                return false;
            }

            if (m.Roles.Select(r => r.Id).Intersect(_ignoredRoles).Any())
            {
                return false;
            }

            return true;
        });

        var usersActivities = new ConcurrentDictionary<ulong, UserActivity>();

        await Parallel.ForEachAsync(guildMembers, async (user, _) =>
        {
            var userActivity = await GetUserActivity(user, dateToCheck);
            usersActivities.AddOrUpdate(user.Id, userActivity, (_, _) => userActivity);
        });

        _cache.ClearCache(dateToCheck);

        var inactiveUsers = usersActivities
            .Values
            .Where(ua => ua.Activity.Sum(a => a.PostCount) < posts)
            .OrderBy(ua => ua.Nickname)
            .ToList();

        var messageBuilder = new StringBuilder();

        if (inactiveUsers.Count == 0)
        {
            messageBuilder.AppendLine($"All users have made at least {posts} post(s), within the last {days} day(s).");
        }
        else
        {
            messageBuilder.AppendLine($"{inactiveUsers.Count} user(s) have not made {posts} post(s), within the last {days} day(s).");
            foreach (var inactiveUser in inactiveUsers)
            {
                var totalPosts = inactiveUser.Activity.Sum(a => a.PostCount);
                if (totalPosts == 0)
                {
                    messageBuilder.AppendLine($"> <@!{inactiveUser.UserId}> has not made any posts, in the last {days} day(s).");
                }else if (totalPosts < posts)
                {
                    var latestPost = inactiveUser.Activity.Max(a => a.LastPost);
                    messageBuilder.AppendLine($"> <@!{inactiveUser.UserId}> has made {totalPosts} post(s), with the last post on <t:{latestPost!.Value.ToUnixTimeSeconds()}:f>.");
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

        _cache.ClearCache(dateToCheck);

        var messageBuilder = new StringBuilder();

        if (userActivity.Activity.Any())
        {
            messageBuilder.AppendLine($"<@!{member.Id}>'s activity, over the last {days} day(s):");
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
            messageBuilder.AppendLine($"<@!{member.Id}> has not made any posts, in the last {days} day(s).");
        }
        

        // ReSharper disable once InvertIf
        if (_erroredChannels.Any())
        {
            messageBuilder.AppendLine("I am unable to access the following channels/threads:");
            foreach (var (erroredChannel, _) in _erroredChannels)
            {
                messageBuilder.AppendLine($"<#{erroredChannel}>");
            }
        }

        return messageBuilder.ToString();
    }

    private async Task<UserActivity> GetUserActivity(IGuildUser member, DateTimeOffset dateToCheck)
    {
        var guild = _client.GetGuild(_config.GuildId);
        var channels = guild.TextChannels.Where(c => !_ignoredChannels.Contains(c.Id));

        var userActivity = new UserActivity {
            UserId = member.Id,
            Nickname = member.Nickname,
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
            _logger.LogInformation("Getting messages.");
            var messages = await _cache.GetMessages(channelId, dateToCheck);
            var channel = (SocketTextChannel)await _client.GetChannelAsync(channelId);

            var userMessages = messages.Where(m => m.Author.Id == userId && m.CreatedAt >= dateToCheck).ToList();

            if (!userMessages.Any())
            {
                _logger.LogInformation("User, {user}, has no messages in channel, {channel}.", userId, channelId);
                return default;
            }

            var latestMessage = userMessages.FirstOrDefault()?.CreatedAt;

            var activity = channel.GetChannelType() == ChannelType.PublicThread ? new UserThreadActivity() : new UserChannelActivity();

            activity.Id = channelId;
            activity.LastPost = latestMessage;
            activity.PostCount = userMessages.Count;
            
            _logger.LogInformation("Found activity, for user, {user}, in channel, {channel}.", userId, channelId);
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
