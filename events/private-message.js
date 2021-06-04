import dotenv from "dotenv";
dotenv.config();

export default async function privateMessage(client) {
    const guild = await client.guilds.fetch(process.env.GUILD);
    const channel = guild.channels.cache.find(ch => ch.id === process.env.CHANNEL);

    client.on("message", message => {
        if (message.channel.type !== "dm") {
            return;
        }
        console.log(message.content);

        channel.send(`Mesage received from ${message.author.username}#${message.author.discriminator}:\r\n>>> ${message}`);
    });
}; 