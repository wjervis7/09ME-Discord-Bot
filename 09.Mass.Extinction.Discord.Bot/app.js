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

client.login("ODUwNDUyNTc4NzA1NzM1NzIw.YLp7rw.wAvnCeCuD5RbtuOEql_hD1km6WM");
