const { Model, DataTypes } = require("sequelize");

class ActivityReport extends Model {
    static sequelizeInit() {
        return {
            id: {
                field: "ActivityReportId",
                type: DataTypes.INTEGER,
                primaryKey: true,
                autoIncrement: true
            },
            initiator: {
                field: "Initiator",
                type: DataTypes.BIGINT
            },
            startTime: {
                field: "StartTime",
                type: DataTypes.DATE
            },
            endTime: {
                field: "EndTime",
                type: DataTypes.DATE
            },
            reportType: {
                field: "ReportType",
                type: DataTypes.TEXT
            },
            args: {
                field: "Args",
                type: DataTypes.TEXT
            },
            report: {
                field: "Report",
                type: DataTypes.TEXT
            }
        }
    }
}

module.exports = ActivityReport;