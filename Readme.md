# Configuration
The following variables need to be set, either through an .env file (if running directly), or through environment variables (needed for Docker):
<table>
    <tr>
        <th>Key</th>
        <th>Description</th>
    </tr>
    <tr>
        <td>TOKEN</td>
        <td>Token for the Discord bot.</td>
    </tr>
    <tr>
        <td>GUILD</td>
        <td>Guild ID that the Discord bot will be in.</td>
    </tr>
    <tr>
        <td>CHANNEL</td>
        <td>Channel ID that the Discord bot will send DM's to.</td>
    </tr>
</table>

# Running the Bot
The bot can be ran two ways: Node.js and Docker.

## Node
1. Clone repo
1. Install dependencies: `npm ci --only=production`
1. Create .env file with config values (see Configuration above)
1. Run app: `node index.js`

## Docker
1. Run image (fill in config values): `docker run -e GUILD= -e CHANNEL= -e TOKEN= wjervis7/09me-discord`
