const { Permissions } = require("discord.js");
const { SlashCommandBuilder } = require("@discordjs/builders");
const moment = require("moment");
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
const getInactiveUsers = async (interaction) => {
    const posts = interaction.options.getInteger("posts");
    const days = interaction.options.getInteger("days");
    const guild = interaction.client.guilds.cache.get(guildId);
    const dateToCheck = moment().startOf("day").subtract(days, "days").valueOf();

    const guildMembers = await guild.members.fetch();
    const channels = await guild.channels.cache;

    const inactiveUsers = [];

    const daysText = days === 1 ? "day" : "days";
    const postsText = posts === 1 ? "post" : "posts";

    for (const member of guildMembers.values()) {
        if (member.user.bot) {
            continue;
        }

        const userActivity = await getUserActivity(member, channels, dateToCheck);
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

    if (!inactiveUsers.length) {
        interaction.editReply(`All users have made at least ${posts} ${postsText}, within the last ${days} ${daysText}.`);
        return;
    }

    const inactiveUsersText = `${inactiveUsers.length} ${inactiveUsers.length === 1 ? "user has" : "users have"}`;

    const message = `
${inactiveUsersText} not made ${posts} ${postsText}, within the last ${days} ${daysText}:${inactiveUsers.join("")}`;

    interaction.editReply(message);
};

// counts number of posts made, by user, in the last x number of days.
const checkUserActivity = async (interaction) => {
    const user = interaction.options.getUser("value");
    const days = interaction.options.getInteger("days");
    const guild = interaction.client.guilds.cache.get(guildId);
    const guildMember = await guild.members.fetch(user.id);
    const channels = await guild.channels.cache;
    const dateToCheck = moment().startOf("day").subtract(days, "days").valueOf();

    const activity = await getUserActivity(user, channels, dateToCheck);

    if (!activity.length) {
        interaction.editReply(`${guildMember.displayName} has not made any posts, in the last ${days} days.`);
        return;
    }

    const message = `
${guildMember.displayName}'s activity, over the last ${days} days:${activity.map(a => `
> In <#${a.channel}>: ${a.posts} ${a.posts.length === 1 ? "post" : "posts"}, with the most recent one on <t:${a.lastPost}:f>.`).join("")}
`;

    interaction.editReply(message);
};

const getUserActivity = async (user, channels, dateToCheck) => {
    const activity = [];

    for (const channel of channels.filter(c => c.type === "GUILD_TEXT").values()) {
        const channelActivity = await getUserMessages(channel, user, dateToCheck);
        activity.push(...channelActivity);

        const activeThreads = await channel.threads.fetchActive();
        const archivedThreads = await channel.threads.fetchArchived();
        const threads = [...activeThreads.threads.values(), ...archivedThreads.threads.values()];
        for (const thread of threads) {
            const threadActivity = await getUserMessages(thread, user, dateToCheck);
            activity.push(...threadActivity);
        }
    }

    return activity;
};

const getUserMessages = async (channelOrThread, user, dateToCheck) => {
    const activity = [];

    const permissions = channelOrThread.permissionsFor(user);
    if (!permissions.has(Permissions.FLAGS.SEND_MESSAGES)) {
        return activity;
    }

    const messages = [...(await channelOrThread.messages.fetch()).filter(m => messageFilter(m, user.id, dateToCheck)).values()];

    if (messages.length) {
        const latestMessage = Math.max.apply(null, messages.map(m => m).map(m => m.createdTimestamp));
        activity.push({
            channel: channelOrThread.id,
            posts: messages.length,
            lastPost: moment(latestMessage).unix()
        });
    }

    return activity;
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

        if (interaction.options.getSubcommand() === "all") {
            return getInactiveUsers(interaction);
        }
        if (interaction.options.getSubcommand() === "user") {
            return checkUserActivity(interaction);
        }
        // shut resharper up
        return 0;
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
