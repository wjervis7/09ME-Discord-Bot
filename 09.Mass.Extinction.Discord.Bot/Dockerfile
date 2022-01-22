FROM node:17.4.0

WORKDIR /usr/src/app

COPY package*.json ./

RUN npm i -g npm@8.3.2 && npm ci --only=production

COPY . .

CMD [ "node", "app.js" ]
