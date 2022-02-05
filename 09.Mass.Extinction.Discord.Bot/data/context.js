const { Sequelize } = require("sequelize");
const Message = require("./entities/Message.js");
const DiscordUser = require("./entities/DiscordUser.js");
const ActivityReport = require("./entities/ActivityReport.js");
const Logger = require("../utilities/logging.js");
const { DB_NAME: dbName, DB_USER: user, DB_PASS: password, DB_HOST: host, DB_PORT: port } = require("../config.json");

const sequelize = new Sequelize(dbName, user, password, {
    host,
    port,
    dialect: "mssql",
    pool: {
        max: 5,
        min: 0,
        acquire: 30000,
        idle: 10000
    },
    define: {
        timestamps: false
    }
});

Message.init(
    Message.sequelizeInit(),
    {
        sequelize,
        tableName: "Messages"
    }
);

DiscordUser.init(
    DiscordUser.sequelizeInit(),
    {
        sequelize,
        tableName: "DiscordUsers"
    }
);

ActivityReport.init(
    ActivityReport.sequelizeInit(),
    {
        sequelize,
        tableName: "ActivityReports"
    }
);

(async () => {
    try {
        await sequelize.authenticate();
        Logger.logInformation("Connection established.");
    } catch (error) {
        Logger.logError("Unable to connect to the database:", error);
    }
})();

class Context {
    async createMessage(sender, body, isAnonymous) {
        const newMessage = await Message.create({
            sender,
            body,
            dateSent: new Date().toISOString(),
            isAnonymous
        });
        return newMessage;
    }

    async updateDiscordUserTimeZone(userId, timeZone) {
        return await DiscordUser.upsert({
            id: userId,
            timeZone
        });
    }

    async getDiscordUserTimeZone(userId) {
        const discordUser = await DiscordUser.findByPk(userId);
        return discordUser.timeZone;
    }

    async getTimeZones() {
        const discordUsers = await DiscordUser.findAll({ attributes: ["timeZone"] });
        return [... new Set(discordUsers.map(discordUser => discordUser.timeZone))];
    }

    async addActivityReport(initiator, startTime, endTime, type, args, report) {
        const r = await ActivityReport.create({
            initiator,
            startTime: startTime,
            endTime: endTime,
            reportType: type,
            args,
            report
        });
        return r;
    }
}

module.exports = Context;
