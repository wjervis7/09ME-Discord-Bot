import sequelize from "sequelize";
const { Model, DataTypes } = sequelize;

class Message extends Model {
    static sequelizeInit() {
        return {
            id: {
                field: "MessageId",
                type: DataTypes.INTEGER,
                primaryKey: true,
                autoIncrement: true
            },
            sender: {
                field: "Sender",
                type: DataTypes.TEXT
            },
            body: {
                field: "Body",
                type: DataTypes.TEXT
            },
            dateSent: {
                field: "DateSent",
                type: DataTypes.DATE
            },
            isAnonymous: {
                field: "IsAnonymous",
                type: DataTypes.BOOLEAN
            }
        }
    }
}

export default Message;
