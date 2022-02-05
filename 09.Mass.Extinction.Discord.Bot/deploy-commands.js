const fs = require("fs");
const { REST } = require("@discordjs/rest");
const { Routes } = require("discord-api-types/v9");
const Logger = require("./utilities/logging.js");
const { clientId, guildId, token, adminRole } = require("./config.json");

const commands = [];
const commandFiles = fs.readdirSync("./commands").filter(file => file.endsWith(".js"));

for (const file of commandFiles) {
    const command = require(`./commands/${file}`);
    commands.push({
        name: command.name,
        data: command.data.toJSON(),
        permissions: command.permissions
    });
}

const rest = new REST({ version: "9" }).setToken(token);

const deployCommands = (async () => {
    try {
        Logger.logInformation("Started refreshing slash commands.");
        const body = commands.map(c => c.data);
        await rest.put(
            Routes.applicationGuildCommands(clientId, guildId),
            { body }
        );

        Logger.logInformation("Successfully reloaded slash commands.");
    } catch (error) {
        Logger.logError(error);
    }
});

const setPermissions = (async(client) => {
    Logger.logInformation("Starting setting permissions for slash commands.");
    const guild = client.guilds.cache.get(guildId);
    const everyone = guild.roles.everyone.id;

    const guildCommands = await rest.get(Routes.applicationGuildCommands(clientId, guildId));

    for (const command of commands) {
        if (!command.permissions) {
            continue;
        }
        const id = guildCommands.find(c => c.name === command.name).id;
        const cmd = await guild.commands.fetch(id);
        const permissions = command.permissions.map(p => {
            const permission = {
                type: p.type,
                permission: p.permission
            };
            if (isNaN(p.id)) {
                if (p.id === "everyone") {
                    permission.id = everyone;
                } else if (p.id === "adminRole") {
                    permission.id = adminRole;
                } else {
                    throw new Error(`Invalid permission: '${permission.id}', for command: '${command.name}'.`);
                }
            } else {
                permission.id = p.id;
            }
            return permission;
        });
        await cmd.permissions.set({ permissions });
    }

    Logger.logInformation("Successfully set permissions for slash commands.");
});

module.exports.deployCommands = deployCommands;
module.exports.setPermissions = setPermissions;
