const Context = require("../data/context.js");
const { guildId, channelId } = require("../config.json");
const context = new Context();

const noBodyText = `
Your message lacks content, so I can't send anything to the Admin Team. Perhaps, you've included:
• GIFs
• Images
• Stickers
If so, edit your message, and try sending it, again.
`;

const anonymousReplyMessageText = "Do you want to send this anonymously? React with 🇾 for yes, or 🇳 for no.";

const messageSentText = "Message was sent to the admin team.";

module.exports = {
    name: "Private Message handler",
    async listen(client) {
        const guild = await client.guilds.fetch(guildId);
        const channel = guild.channels.cache.find(ch => ch.id === channelId);

        client.on("messageCreate",
            async (message) => {
                const { author: { username, discriminator, id: authorId }, channel: { type }, content } = message;

                if (type !== "DM" || message.author.bot) {
                    return;
                }

                try {
                    if (content.length === 0) {
                        await message.reply(noBodyText);
                        return;
                    }

                    const reply = await message.reply(anonymousReplyMessageText);
                    await reply.react("🇾");
                    await reply.react("🇳");

                    const filter = (reaction, user) => {
                        return (reaction.emoji.name === "🇾" || reaction.emoji.name === "🇳") && user.id === authorId;
                    };

                    const collector = reply.createReactionCollector({ filter, time: 15_000 });
                    collector.on("collect", async(reaction) => {
                        const isAnonymous = reaction.emoji.name === "🇾";
                        console.log(`Message is anonymous: ${isAnonymous}.`);
                        const sender = `${username}#${discriminator}`;
                        try {
                            const newMessage = await context.createMessage(sender, content, isAnonymous);
                            console.log(newMessage);

                            if (isAnonymous) {
                                await channel.send(`Anonymous message received:\n>>> ${content}`);
                            } else {
                                await channel.send(`Message received from ${sender}:\n>>> ${content}`);
                            }

                            collector.stop();
                            await reply.channel.send(messageSentText);
                        } catch (collectorError) {
                            console.error("An error has occurred :(", collectorError);
                        }
                    });
                } catch (error) {
                    console.error("An error has occurred :(.", error);
                }
            });
    }
};
