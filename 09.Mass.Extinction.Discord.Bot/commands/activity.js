const { SlashCommandBuilder, Embed } = require("@discordjs/builders");
const moment = require("moment");
const { to } = require("../utilities/asyncHelpers.js");
const { sleep } = require("../utilities/helpers.js");
const Logger = require("../utilities/logging.js");
const Context = require("../data/context.js");
const { restrictions, activitySettings: { excludedChannels, excludedUsers } } = require("../config.json");

const cache = {
    lastInsert: null,
    messages: {}
};

const context = new Context();

const command = new SlashCommandBuilder()
    .setName("activity")
    .setDescription("Gets user activity for Discord.")
    .setDefaultPermission(false)
    .addSubcommand(subcommand =>
        subcommand
        .setName("all")
        .setDescription("Gets all users that haven't made x number of posts, in the last y days.")
        .addIntegerOption(option =>
            option
            .setName("posts")
            .setDescription("The minimum number of posts, that the user must have made, to be active. Value is inclusive.")
            .setRequired(true)
        )
        .addIntegerOption(option =>
            option
            .setName("days")
            .setDescription("The number of days, that the user must have posted within, to be active.  Value is inclusive.")
            .setRequired(true)
        )
    )
    .addSubcommand(subcommand =>
        subcommand
        .setName("user")
        .setDescription("Gets how active a user is, within the last x days.")
        .addUserOption(option =>
            option
            .setName("value")
            .setDescription("The user to check activity for.")
            .setRequired(true)
        )
        .addIntegerOption(option =>
            option
            .setName("days")
            .setDescription("The number of days, that the user must have posted within, to be active.  Value is inclusive.")
            .setRequired(true)
        )
    );

const allowedChannels = restrictions.find(r => r.command === command.name).channels;

// gets all users that haven't made x number of posts, in the last y number of days.
const getInactiveUsers = async (guild, posts, days, dateToCheck) => {
    let error;
    let guildMembers;
    let channels;
    [error, { guildMembers, channels }] = await to(getGuildMembersAndChannels(guild));
    if (error) {
        return error;
    }

    const inactiveUsers = [];

    const daysText = days === 1 ? "day" : "days";
    const postsText = posts === 1 ? "post" : "posts";

    const errors = new Set();

    const memberCount = guildMembers.length;
    let user = 0;
    for (const member of guildMembers) {
        Logger.logInformation(`Processing user ${++user}/${memberCount}.`);

        if (member.user.bot) {
            continue;
        }

        if (excludedUsers.includes(member.user.id)) {
            Logger.logVerbose(`User is in exclusion list; skipping user ${member.displayName}.`);
            continue;
        }

        Logger.logVerbose(`Checking user ${member.displayName}`);

        let userActivity;

        [error, userActivity] = await to(getUserActivity(member, channels, dateToCheck, errors));

        if (error) {
            return error;
        }

        const totalPosts = userActivity.reduce((sum, c) => sum + c.posts, 0);
        if (totalPosts === 0) {
            inactiveUsers.push(`
> ${member.displayName} has not made any posts.`);
        }
        else if (totalPosts < posts) {
            const latestPost = Math.max.apply(null, userActivity.map(a => a.lastPost));
            inactiveUsers.push(`
> ${member.displayName} has made ${totalPosts} ${totalPosts === 1 ? "post" : "posts"}, with the last post on <t:${latestPost}:f>.`);
        }
    }

    let errorMessage = "";

    if (errors.size) {
        errorMessage = `
I am unable to access the following channels/threads:${[...errors].map(e => `
<#${e}>`).join("")}`;
    }

    if (!inactiveUsers.length) {
        return `All users have made at least ${posts} ${postsText}, within the last ${days} ${daysText}.
${errorMessage}`;
    }

    const inactiveUsersText = `${inactiveUsers.length} ${inactiveUsers.length === 1 ? "user has" : "users have"}`;

    return `
${inactiveUsersText} not made ${posts} ${postsText}, within the last ${days} ${daysText}:${inactiveUsers.join("")}
${errorMessage}`;
};

// counts number of posts made, by user, in the last x number of days.
const checkUserActivity = async (guild, user, days, dateToCheck) => {
    let error;
    let guildMember;
    let channels;
    let activity;

    [error, { guildMembers: guildMember, channels }] = await to(getGuildMembersAndChannels(guild, user.id));
    if (error) {
        return error;
    }

    const errors = new Set();
    Logger.logInformation(`Processing user ${guildMember.nickname || guildMember.user.username}`);

    [error, activity] = await to(getUserActivity(user, channels, dateToCheck, errors));

    if (error) {
        return error;
    }

    let errorMessage = "";

    if (errors.size) {
        errorMessage = `
I am unable to access the following channels:${[...errors].map(e => `
<#${e}>`).join("")}`;
    }

    activity = activity.filter(a => a.posts > 0);

    if (!activity.length) {
        return `${guildMember.displayName} has not made any posts, in the last ${days} days.
${errorMessage}`;
    }
    activity = activity.map(a => {
        if (a.type === "c") {
            return `
> In <#${a.channel}>: ${a.posts} ${a.posts.length === 1 ? "post" : "posts"}, with the most recent one on <t:${a.lastPost}:f>.`;
        }
        return `
> In <#${a.channel}>(thread ${a.name}, ${a.type}) : ${a.posts} ${a.posts.length === 1 ? "post" : "posts"}, with the most recent one on <t:${a.lastPost}:f>.`;
    });
    return `
${guildMember.displayName}'s activity, over the last ${days} days:${activity.join("")}
${errorMessage}`;
};

const getGuildMembersAndChannels = async(guild, memberId) => {
    let error;
    let guildMembers;
    let channels;

    [error, guildMembers] = await to(guild.members.fetch(memberId));
    if (error) {
        return `Unable to get user(s) from server:
\`\`\`
${error.stack}
\`\`\`
`;
    }

    [error, channels] = await to(guild.channels.fetch());
    if (error) {
        return `Unable to get channels from server:
\`\`\`
${error.stack}
\`\`\`
`;
    }

    return { guildMembers: memberId ? guildMembers : [...guildMembers.values()], channels: [...channels.values()] };
};

const getUserActivity = async (user, channels, dateToCheck, errors) => {
    const activity = [];

    for (const channel of channels.filter(c => c.type === "GUILD_TEXT").values()) {
        if (errors.has(channel.id)) {
            Logger.logVerbose(`Channel ${channel.name} has errored before; skipping channel.`);
            continue;
        }

        if (excludedChannels.includes(channel.id)) {
            Logger.logVerbose(`Channel is in exclusion list; skipping channel ${channel.name}.`);
            continue;
        }

        Logger.logVerbose(`Checking channel ${channel.name}`);

        let error;
        let channelActivity;

        [error, channelActivity] = await to(getUserMessages(channel, user, dateToCheck));

        if (error) {
            errors.add(channel.id);
            continue;
        }

        channelActivity.type = "c";
        activity.push(channelActivity);

        const activeThreads = await channel.threads.fetchActive();
        const archivedThreads = await channel.threads.fetchArchived();
        const threads = [...activeThreads.threads.values(), ...archivedThreads.threads.values()];
        for (const thread of threads) {
            if (errors.has(thread.id)) {
                Logger.logVerbose(`Thread ${thread.name} has errored before; skipping thread.`);
                continue;
            }

            Logger.logVerbose(`Checking thread ${thread.name}`);

            let threadActivity;

            [error, threadActivity] = await to(getUserMessages(thread, user, dateToCheck));

            if (error) {
                errors.add(thread.id);
                continue;
            }

            threadActivity.type = thread.archived ? "archived" : "active";
            threadActivity.name = thread.name;
            activity.push(threadActivity);
        }
    }

    return activity;
};

const getUserMessages = async (channelOrThread, user, dateToCheck) => {
    let [error, fetchedMessages] = await to(channelOrThread.messages.fetch({ limit: 100 }));

    if (error) {
        throw `Unable to get messages from channel/thread: <#${channelOrThread.id}> (${error.path}):
\`\`\`
${error.stack}
\`\`\`
`;
    }

    let messages;
    const cacheKey = `${channelOrThread.id}.${dateToCheck}`;

    if (cache.lastInsert && moment().isBefore(cache.lastInsert) && cache.messages[cacheKey]) {
        messages = cache.messages[cacheKey];
        Logger.logVerbose("Using cached messages.");
    } else {
        Logger.logVerbose("No cached messages, or cache is old.  Getting new messages.");
        messages = [...fetchedMessages.values()];

        let atLimit = messages.length === 100;
        let oldestMessage = messages.at(-1);

        while (atLimit && oldestMessage.createdTimestamp >= dateToCheck) {
            fetchedMessages = await channelOrThread.messages.fetch({ limit: 100, before: oldestMessage.id });
            atLimit = fetchedMessages.size === 100;
            oldestMessage = fetchedMessages.at(- 1);
            messages = messages.concat([...fetchedMessages.values()]);
        }

        Logger.logVerbose("Adding messages to cache.");
        cache.lastInsert = moment().add(15, "minutes");
        cache.messages[cacheKey] = messages;
    }

    const userMessages = [...messages.filter(m => messageFilter(m, user.id, dateToCheck)).values()];

    const latestMessage = userMessages[0] ? userMessages[0].createdTimestamp : undefined;
    return {
        channel: channelOrThread.id,
        posts: userMessages.length,
        lastPost: latestMessage ? moment(latestMessage).unix() : undefined
    };
};

const messageFilter = (message, userId, dateToCheck) => message.author.id === userId && message.createdTimestamp >= dateToCheck;

module.exports = {
    name: command.name,
    data: command,
    async execute(interaction) {
        if (!allowedChannels.includes(interaction.channelId)) {
            await interaction.reply({ content: "This command must be used in an admin channel.", ephemeral: true });
            return;
        }

        // 14 minutes 30 seconds
        const time = (14 * 60 * 1000) + (30 * 1000);
        let timedOut = false;
        const timeOut = setTimeout(() => {
                interaction.editReply("This report is taking too long. I'll send a message, to this channel, once it is complete.");
                timedOut = true;
            },
            time
        );

        const channel = interaction.channel;
        await interaction.deferReply("Getting user activity. This might take some time.");
        const startTime = moment().toISOString();
        const guild = interaction.guild;
        const days = interaction.options.getInteger("days");
        const dateToCheck = moment().startOf("day").subtract(days, "days").valueOf();
        let type = interaction.options.getSubcommand();
        const args = { days };

        let response = "";
        let error;

        if (type === "all") {
            const posts = interaction.options.getInteger("posts");
            args.posts = posts;
            [error, response] = await to(getInactiveUsers(guild, posts, days, dateToCheck));
        }
        if (type === "user") {
            const user = interaction.options.getUser("value");
            args.user = user.id;
            [error, response] = await to(checkUserActivity(guild, user, days, dateToCheck));
        }

        const endTime = moment().toISOString();

        response = response
            ? response
            : error
            ? `${error.message}:
    ${error.stack}`
            : "Error, no content.";

        type = type === "all" ? `All Users` : `Single User`;

        await context.addActivityReport(interaction.user.id, startTime, endTime, type, JSON.stringify(args), response);

        if (!timedOut) {
            clearTimeout(timeOut);
            if (error) {
                interaction.editReply(error.message);
            } else {
                interaction.editReply(response);
            }
        } else {

            const message = new Embed()
                .addField({ name: "Activity Report", value: response });

            await channel.send({ embeds: [message] });
        }
    },
    permissions: [
        {
            id: "everyone",
            type: "ROLE",
            permission: false
        },
        {
            id: "adminRole",
            type: "ROLE",
            permission: true
        }
    ]
};
