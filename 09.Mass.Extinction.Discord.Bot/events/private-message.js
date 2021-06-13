import dotenv from "dotenv";
import "discord-reply"; // not sure if needed, since it was added in index.js
import Context from "../data/context.js";

const context = new Context();
dotenv.config();

export default async function privateMessage(client) {
    const guild = await client.guilds.fetch(process.env.GUILD);
    const channel = guild.channels.cache.find(ch => ch.id === process.env.CHANNEL);

    client.on("message", async (message) => {
        const { author: { username, discriminator, id: authorId }, channel: { type }, content } = message;

        if (type !== "dm" || message.author.bot) {
            return;
        }

        try {
            const reply =
                await message.lineReply("Do you want to send this anonymously? React with 🇾 for yes, or 🇳 for no.");
            await reply.react("🇾");
            await reply.react("🇳");

            const filter = (reaction, user) => user.id === authorId &&
                (reaction.emoji.name === "🇾" || reaction.emoji.name === "🇳");
            const collector = reply.createReactionCollector(filter, { time: 15000 });
            collector.on("collect", async (reaction) => {
                const isAnonymous = reaction.emoji.name === "🇾";
                const sender = `${username}#${discriminator}`;
                try {
                    const newMessage = await context.createMessage(sender, content, isAnonymous);
                    console.log(newMessage);

                    if (isAnonymous) {
                        await channel.send(`Anonymous mesage received:\n>>> ${content}`);
                    } else {
                        await channel.send(`Mesage received from ${sender}:\n>>> ${content}`);
                    }

                    collector.stop();
                    await reply.reply("Message was sent to the admin team.");
                } catch (innerError) {
                    console.error("An error has occurred :(.", innerError);
                }
            });
        } catch (error) {
            console.error("An error has occurred :(.", error);
        }
    });
};
