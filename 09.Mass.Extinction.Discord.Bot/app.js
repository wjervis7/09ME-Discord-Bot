const fs = require("fs");
const { Client, Collection, Intents } = require("discord.js");
const { REST } = require("@discordjs/rest");
const { Routes } = require("discord-api-types/v9");
const { token, clientId, guildId, adminRole } = require("./config.json");

// import privateMessage from "./events/private-message.js";


const client = new Client({ intents: [Intents.FLAGS.GUILDS] });

client.commands = new Collection();
const commandFiles = fs.readdirSync("./commands").filter(file => file.endsWith(".js"));
for (const file of commandFiles) {
    const command = require(`./commands/${file}`);
    client.commands.set(command.data.name, command);
}

client.once("ready", async () => {
    console.log("Client is ready.");

    // update slash command permissions
    await setSlashCommandPermissions();
    
    // register event handlers
    // await privateMessage(client);
});

client.on("interactionCreate", async interaction => {
    if (!interaction.isCommand()) return;

    const command = client.commands.get(interaction.commandName);
    
    if (!command) return;

    try {
        await command.execute(interaction);
    } catch (error) {
        console.error(error);
        return interaction.reply({ content: "There was an error while executing this command!", ephemeral: true });
    }
});

client.login(token);

const setSlashCommandPermissions = async () => {
    const guild = client.guilds.cache.get(guildId);
    const everyone = guild.roles.everyone.id;
    
    const rest = new REST({ version: "9" }).setToken(token);
    const guildCommands = await rest.get(Routes.applicationGuildCommands(clientId, guildId));

    // yogie
    const yogieId = guildCommands.find(command => command.name === "yogie").id;
    const yogieCommand = await guild.commands.fetch(yogieId);
    const yogiePermissions = [
        {
            id: everyone,
            type: "ROLE",
            permission: false
        },
        {
            id: adminRole,
            type: "ROLE",
            permission: true
        }
    ];
    await yogieCommand.permissions.set({ permissions: yogiePermissions });
}
