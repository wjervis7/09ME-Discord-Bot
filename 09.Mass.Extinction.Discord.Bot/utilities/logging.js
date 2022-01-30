let { loggingLevel } = require("../config.json");

const LogLevels = {
    10: "Verbose",
    20: "Information",
    30: "Warning",
    40: "Error"
};

const DEFAULT = 30;

const getLogLevel = level => {
    if (isNaN(level)) {
        const logLevel = Object.entries(LogLevels).map(([key, value]) => {
            return { key, value };
        }).find(({ value }) => value === level);
        if (logLevel) {
            return logLevel.key;
        }
    }

    if (LogLevels[level]) {
        return level;
    }

    return DEFAULT;
};

loggingLevel = getLogLevel(loggingLevel);

const log = (message, level) => {
    const logLevel = getLogLevel(level);
    if (logLevel >= loggingLevel) {
        console.log(message);
    }
};

const Logger = {
    log,
    logVerbose: message => log(message, LogLevels[10]),
    logInformation: message => log(message, LogLevels[20]),
    logWarning: message => log(message, LogLevels[30]),
    logError: message => log(message, LogLevels[40])
};

module.exports = Logger;
