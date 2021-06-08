import dotenv from "dotenv";
import "discord-reply"; // not sure if needed, since it was added in index.js

dotenv.config();

export default async function privateMessage(client) {
    const guild = await client.guilds.fetch(process.env.GUILD);
    const channel = guild.channels.cache.find(ch => ch.id === process.env.CHANNEL);

    client.on("message", async (message) => {
        const { author: { username, discriminator, id: authorId }, channel: { type }, content } = message;

        if (type !== "dm" || message.author.bot) {
            return;
        }

        const reply = await message.lineReply("Do you want to send this anonymously? React with 🇾 for yes, or 🇳 for no.");
        await reply.react("🇾");
        await reply.react("🇳");

        const filter = (reaction, user) => user.id === authorId && (reaction.emoji.name === "🇾" || reaction.emoji.name === "🇳");
        const collector = reply.createReactionCollector(filter, { time: 15000 });
        collector.on("collect", async (reaction) => {
            if (reaction.emoji.name === "🇾") {
                await channel.send(`Anonymous mesage received:\n>>> ${content}`);
            }
            else {
                await channel.send(`Mesage received from ${username}#${discriminator}:\n>>> ${content}`);
            }

            collector.stop();
            await reply.reply("Message was sent to the admin team.");
        });
    });
};
