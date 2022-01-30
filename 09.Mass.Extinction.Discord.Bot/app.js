const fs = require("fs");
const { Client, Collection, Intents } = require("discord.js");
const commandHelper = require("./deploy-commands.js");
const Logger = require("./utilities/logging.js");
const { token } = require("./config.json");

const client = new Client({
    intents: [Intents.FLAGS.GUILDS, Intents.FLAGS.GUILD_MEMBERS, Intents.FLAGS.DIRECT_MESSAGES, Intents.FLAGS.DIRECT_MESSAGE_REACTIONS],
    partials: ["CHANNEL"]
});

client.commands = new Collection();
const commandFiles = fs.readdirSync("./commands").filter(file => file.endsWith(".js"));
for (const file of commandFiles) {
    const command = require(`./commands/${file}`);
    client.commands.set(command.data.name, command);
}

client.once("ready", async () => {
    Logger.logInformation("Client is ready.");

    // deploy slash commands
    await commandHelper.deployCommands();

    // update slash command permissions
    await commandHelper.setPermissions(client);

    // register event handlers
    const events = fs.readdirSync("./events").filter(file => file.endsWith(".js"));
    for (const file of events) {
        const event = require(`./events/${file}`);
        Logger.logInformation(`Starting event handler '${event.name}'.`);
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
        Logger.logError(error);
        const message = "There was an error while executing this command!";
        if (interaction.deferred && interaction.ephemeral) {
            interaction.editReply({ content: message, ephemeral: true });
        } else if (interaction.deferred) {
            interaction.editReply(message);
        } else if (interaction.ephemeral) {
            interaction.reply({ content: message, ephemeral: true });
        } else {
            interaction.reply(message);
        }
    }
});

client.login(token);
