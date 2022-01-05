const fs = require("fs");
const { REST } = require("@discordjs/rest");
const { Routes } = require("discord-api-types/v9");
const { clientId, guildId, token, adminRole } = require("./config.json");

const commands = [];
const commandFiles = fs.readdirSync("./commands").filter(file => file.endsWith(".js"));

for (const file of commandFiles) {
    const command = require(`./commands/${file}`);
    commands.push(command.data.toJSON());
}

const rest = new REST({ version: "9" }).setToken(token);

const deployCommands = (async () => {
    try {
        console.log("Started refreshing slash commands.");
        await rest.put(
            Routes.applicationGuildCommands(clientId, guildId),
            { body: commands }
        );

        console.log("Successfully reloaded slash commands.");
    } catch (error) {
        console.error(error);
    }
});

const setPermissions = (async(client) => {
    console.log("Starting setting permissions for slash commands.");
    const guild = client.guilds.cache.get(guildId);
    const everyone = guild.roles.everyone.id;

    const guildCommands = await rest.get(Routes.applicationGuildCommands(clientId, guildId));

    // Example below
    //// yogie
    //const yogieId = guildCommands.find(command => command.name === "yogie").id;
    //const yogieCommand = await guild.commands.fetch(yogieId);
    //const yogiePermissions = [
    //    {
    //        id: everyone,
    //        type: "ROLE",
    //        permission: false
    //    },
    //    {
    //        id: adminRole,
    //        type: "ROLE",
    //        permission: true
    //    }
    //];
    //await yogieCommand.permissions.set({ permissions: yogiePermissions });

    console.log("Successfully set permissions for slash commands.");
});

module.exports.deployCommands = deployCommands;
module.exports.setPermissions = setPermissions;
