const { Model, DataTypes } = require("sequelize");

class DiscordUser extends Model {
    static sequelizeInit() {
        return {
            id: {
                field: "DiscordUserId",
                type: DataTypes.TEXT,
                primaryKey: true,
                autoIncrement: false
            },
            timeZone: {
                field: "TimeZone",
                type: DataTypes.TEXT
            }
        }
    }
}

module.exports = DiscordUser;