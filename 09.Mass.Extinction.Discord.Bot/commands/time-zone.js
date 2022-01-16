const { SlashCommandBuilder } = require("@discordjs/builders");
const moment = require("moment-timezone");
const Context = require("../data/context.js");
const timezones = require("../timezones.json");
const context = new Context();

const similarZonesFoundMessage = `
I could not find your time zone.
However, it might match one of these time zones:
{zones}
Try the command again, with the time zone, from this list, or, find it here: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.`;
const zoneNotFoundMessage = `
I could not find your time zone, in my database.
Try finding it here, and use the command again, with the database name:  https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.
It will look like 'America/Los_Angeles'.`;
const zoneSavedMessage = `Your time zone has been saved!`;
const zoneRemovedMessage = `Your time zone has been removed!`;

const command = new SlashCommandBuilder()
    .setName("time-zone")
    .setDescription("Set or remove your timezone.")
    .addSubcommand(subcommand =>
        subcommand
        .setName("set")
        .setDescription("Sets your time zone.")
        .addStringOption(option =>
            option
            .setName("zone")
            .setDescription("The name of the time zone, as described by the IANA tz database.").setRequired(true)
        )
    )
    .addSubcommand(subcommand =>
        subcommand
        .setName("remove")
        .setDescription("Removes your time zone.")
    );

const setTimezone = async(interaction) => {
    const { user: { id: userId } } = interaction;
    
    const timeZone = interaction.options.getString("zone");

    const zone = moment.tz.zone(timeZone);

    if (!zone) {
        let similarZones = findSimilarTimeZones(timeZone);

        if (similarZones.length > 0) {
            similarZones = similarZones.join("\r\n");
            await interaction.reply({
                content: similarZonesFoundMessage.replace("{zones}", similarZones),
                ephemeral: true
            });
        } else {
            await interaction.reply({
                content: zoneNotFoundMessage,
                ephemeral: true
            });
        }
        return;
    }

    await context.updateDiscordUserTimeZone(userId, zone.name);
    await interaction.reply({ content: zoneSavedMessage, ephemeral: true });
};

const removeTimezone = async(interaction) => {
    const { user: { id: userId } } = interaction;

    await context.updateDiscordUserTimeZone(userId, null);
    await interaction.reply({ content: zoneRemovedMessage, ephemeral: true });
};

const findSimilarTimeZones = (timeZone) => {
    let similarZones = timezones
        .filter(tz => similarTimeZoneFilter(tz, timeZone))
        .map(tz => tz.utc.filter(s => s.includes("/")))
        .flat();
    similarZones = [... new Set(similarZones)];
    similarZones = similarZones.sort((a, b) => a.localeCompare(b));

    return similarZones;
};

const similarTimeZoneFilter = (tz, timeZone) => {
    const re = new RegExp(`\b${timeZone}\b`, "i");
    return tz.value === timeZone ||
        re.test(tz.value) ||
        tz.abbr === timeZone ||
        re.test(tz.abbr) ||
        tz.text === timeZone ||
        re.test(tz.text);
};

module.exports = {
    name: command.name,
    data: command,
    async execute(interaction) {
        interaction.ephemeral = true;
        const subCommand = interaction.options.getSubcommand();
        if (subCommand === "set") {
            await setTimezone(interaction);
        }else if (subCommand === "remove") {
            await removeTimezone(interaction);
        }
    }
};
