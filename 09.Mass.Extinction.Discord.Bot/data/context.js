import { Sequelize } from "sequelize";
import Message from "./entities/Message.js";
import dotenv from "dotenv";

dotenv.config();

const sequelize = new Sequelize(process.env.DB_NAME, process.env.DB_USER, process.env.DB_PASS, {
    host: process.env.DB_HOST,
    port: process.env.DB_PORT,
    dialect: "mssql",
    operatorsAliases: false,
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





(async () => {
    try {
        await sequelize.authenticate();
        console.log("Connection established.");
    } catch (error) {
        console.error("Unable to connect to the database:", error);
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
}

export default Context;
