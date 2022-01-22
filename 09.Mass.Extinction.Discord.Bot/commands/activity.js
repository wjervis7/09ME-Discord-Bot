const { SlashCommandBuilder } = require("@discordjs/builders");
const moment = require("moment");
const { to } = require("../utilities/asyncHelpers.js");
const { guildId, restrictions } = require("../config.json");

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

    for (const member of guildMembers.values()) {
        if (member.user.bot) {
            continue;
        }
        console.info(`Checking user ${member.displayName}`);

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

    return { guildMembers, channels };
};

const getUserActivity = async (user, channels, dateToCheck, errors) => {
    const activity = [];

    for (const channel of channels.filter(c => c.type === "GUILD_TEXT").values()) {
        if (errors.has(channel.id)) {
            console.info(`Channel ${channel.name} has errored before; skipping channel.`);
            continue;
        }

        console.info(`Checking channel ${channel.name}`);

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
                console.info(`Thread ${thread.name} has errored before; skipping thread.`);
                continue;
            }

            console.info(`Checking thread ${thread.name}`);

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

    let messages = [...fetchedMessages.values()];

    let atLimit = messages.length === 100;
    let oldestMessage = messages.at(-1);

    while (atLimit && oldestMessage.createdTimestamp >= dateToCheck) {
        fetchedMessages = await channelOrThread.messages.fetch({ limit: 100, before: oldestMessage.id });
        atLimit = fetchedMessages.size === 100;
        oldestMessage = fetchedMessages.at(- 1);
        messages = messages.concat([...fetchedMessages.values()]);
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

        await interaction.deferReply("Getting user activity. This might take some time.");
        const guild = interaction.client.guilds.cache.get(guildId);
        const days = interaction.options.getInteger("days");
        const dateToCheck = moment().startOf("day").subtract(days, "days").valueOf();

        let response = "";
        let error;

        if (interaction.options.getSubcommand() === "all") {
            [error, response] = await to(getInactiveUsers(guild, interaction.options.getInteger("posts"), days, dateToCheck));
        }
        if (interaction.options.getSubcommand() === "user") {
            [error, response] = await to(checkUserActivity(guild, interaction.options.getUser("value"), days, dateToCheck));
        }

        if (error) {
            interaction.editReply(error);
        } else {
            interaction.editReply(response);
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
