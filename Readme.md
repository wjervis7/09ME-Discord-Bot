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

# Features
 - Users can DM the bot to send a message to the admin team.  Bot will prompt the user if they want the message to be sent anonymously.  Message will be sent to a predetermined channel.
 
 # Development

 ## Requirements

  - An IDE (e.g., [Visual Studio Code](https://code.visualstudio.com/download))
  - [Node.js](https://nodejs.org/en/)
  - A Git client (e.g., [GitHub Desktop](https://desktop.github.com/))
  - (Optional) [Docker](https://docs.docker.com/get-docker/)

## Instructions

1. Clone the GitHub repo 
1. Create a new branch based off dev branch
1. Install dependencies: `npm i`
1. (Optional) install nodemon globally: `npm i --global nodemon`
1. Develop
1. Push changes to repo
1. Create a pull request to merge your branch to dev
