const { SlashCommandBuilder } = require("@discordjs/builders");
const moment = require("moment-timezone");
const Context = require("../data/context.js");
const Logger = require("../utilities/logging.js");
const context = new Context();

const invalidTimeMessage = `The time you provided, is not a valid time format.`;

const command = new SlashCommandBuilder()
    .setName("time")
    .setDescription("Displays the provided time, in all time zones, that have been saved, by users.")
    .addStringOption(option =>
        option
        .setName("value")
        .setDescription("The date/time you want to have displayed.").setRequired(true)
    )
    .addStringOption(option =>
        option
        .setName("format")
        .setDescription("Display time, date, or both.")
        .setRequired(true)
        .addChoice("Time", "time")
        .addChoice("Date and Time", "datetime")
        .addChoice("Date", "date")
    );

module.exports = {
    name: command.name,
    data: command,
    async execute(interaction) {
        const time = interaction.options.getString("value");
        const format = interaction.options.getString("format");
        const userId = interaction.user.id;
        const userTimezone = await context.getDiscordUserTimeZone(userId);
        const allTimezones = (await context.getTimeZones()).filter(tz => tz !== userTimezone);
        Logger.logInformation(userTimezone);
        Logger.logInformation(allTimezones);

        moment.tz.setDefault(userTimezone);
        let m = moment(time);
        if (!m.isValid()) {
            m = moment.tz(time, ["h a", "h:mm a", moment.HTML5_FMT.TIME, moment.HTML5_FMT.DATE], userTimezone);
            if (!m.isValid()) {
                interaction.reply(invalidTimeMessage);
            }
        }
        Logger.logVerbose(m.format());

        let momentFormat = "";
        switch (format) {
        case "date":
            momentFormat = "LL";
            break;
        case "time":
            momentFormat = "LT z";
            break;
        case "datetime":
            momentFormat = "LL LT z";
            break;
        }


        let output = `
You entered: ${m.format(momentFormat)}.
This time, in other timezones: `;
        for (const zone of allTimezones) {
            output += `
${m.tz(zone).format(momentFormat)}`;
        }

        output += `
_If you want your timezone added to this list, use the \`/time-zone set\` command._`; 

        interaction.reply(output);
    }
};
