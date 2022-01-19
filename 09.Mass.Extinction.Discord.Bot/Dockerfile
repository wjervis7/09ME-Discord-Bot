FROM node:17

WORKDIR /usr/src/app

COPY package*.json ./

RUN npm i -g npm@8.3.1 && npm ci --only=production && npm audit fix

COPY . .

CMD [ "node", "app.js" ]
