const { SlashCommandBuilder } = require("@discordjs/builders");
const { guildId, adminUserId, restrictions } = require("../config.json");

const command = new SlashCommandBuilder()
    .setName("nickname")
    .setDescription("Sets nickname for a user.")
    .setDefaultPermission(false)
    .addUserOption(option =>
        option
        .setName("user")
        .setDescription("The user whose nickname will be changed.")
        .setRequired(true)
    )
    .addStringOption(option =>
        option
        .setName("name")
        .setDescription("The new nickname for the user.")
        .setRequired(true)
    );

const allowedChannels = restrictions.find(r => r.command === command.name).channels;

module.exports = {
    name: command.name,
    data: command,
    async execute(interaction) {
        if (!allowedChannels.includes(interaction.channelId)) {
            await interaction.reply({ content: "This command must be used in an admin channel.", ephemeral: true });
            return;
        }
        interaction.ephemeral = true;
        const user = interaction.options.getUser("user");
        const nickname = interaction.options.getString("name");
        const guild = interaction.client.guilds.cache.get(guildId);
        const guildMember = guild.members.cache.get(user.id);
        const oldNickname = guildMember.displayName;
        await guildMember.setNickname(nickname);
        interaction.reply({content: `${oldNickname}'s nickname has been changed to ${guildMember.displayName}.`, ephemeral: true });
    },
    permissions: [
        {
            id: "everyone",
            type: "ROLE",
            permission: false
        },
        {
            id: adminUserId,
            type: "USER",
            permission: true
        }
    ]
};
