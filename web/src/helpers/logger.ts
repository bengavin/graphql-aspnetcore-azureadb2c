import { keys } from "@global/config";

export const loggingLevel = {
    TRACE: 0,
    DEBUG: 1,
    INFO: 2,
    WARN: 3,
};

export const logger = {
    initialize: () => {
    },
    log: (name: string, properties?: any, level: number = loggingLevel.DEBUG) => {
        const logLevel = loggingLevel[keys.LOG_LEVEL];

        if (logLevel <= level) {
            console.log(name, properties);
        }
    },
    error: (error: Error, properties?: any) => {
        console.error(error, properties);
    },
};
