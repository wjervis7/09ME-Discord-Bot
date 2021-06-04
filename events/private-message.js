import dotenv from "dotenv";
dotenv.config();

export default async function privateMessage(client) {
    const guild = await client.guilds.fetch(process.env.GUILD);
    const channel = guild.channels.cache.find(ch => ch.id === process.env.CHANNEL);

    client.on("message", message => {
        const { author: { username, discriminator }, channel: { type }, content } = message;

        if (type !== "dm") {
            return;
        }

        console.log(content);

        channel.send(`Mesage received from ${username}#${discriminator}:\n>>> ${message}`);
    });
};
