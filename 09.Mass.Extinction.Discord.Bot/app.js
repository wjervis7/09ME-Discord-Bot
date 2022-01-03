const fs = require("fs");
const { Client, Collection, Intents } = require("discord.js");
const commandHelper = require("./deploy-commands.js");
const { token } = require("./config.json");

// import privateMessage from "./events/private-message.js";


const client = new Client({
    intents: [Intents.FLAGS.GUILDS, Intents.FLAGS.DIRECT_MESSAGES, Intents.FLAGS.DIRECT_MESSAGE_REACTIONS],
    partials: ["CHANNEL"]
});

client.commands = new Collection();
const commandFiles = fs.readdirSync("./commands").filter(file => file.endsWith(".js"));
for (const file of commandFiles) {
    const command = require(`./commands/${file}`);
    client.commands.set(command.data.name, command);
}

client.once("ready", async () => {
    console.log("Client is ready.");

    // deploy slash commands
    await commandHelper.deployCommands();

    // update slash command permissions
    await commandHelper.setPermissions(client);

    // register event handlers
    const events = fs.readdirSync("./events").filter(file => file.endsWith(".js"));
    for (const file of events) {
        const event = require(`./events/${file}`);
        console.log(`Starting event handler '${event.name}'.`);
        event.listen(client);
    }
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
