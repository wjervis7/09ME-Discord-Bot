const { SlashCommandBuilder } = require("@discordjs/builders");

module.exports = {
    data: new SlashCommandBuilder()
        .setName("yogie")
        .setDescription("Says hello, from Frozen Yogie.")
        .setDefaultPermission(false),
    async execute(interaction) {
        await interaction.reply("Hello! I am Frozen Yogie!");
    }
};
