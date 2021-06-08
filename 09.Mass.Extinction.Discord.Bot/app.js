"use strict";

import { Client } from "discord.js";
import "discord-reply";
import dotenv from "dotenv";
import privateMessage from "./events/private-message.js";

dotenv.config();

const client = new Client();

client.on("ready", async () => {
    console.log("Client is ready.");

    // register event handlers
    await privateMessage(client);
});

client.login(process.env.TOKEN);
