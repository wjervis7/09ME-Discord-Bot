FROM node

WORKDIR /usr/src/app

COPY package*.json ./

RUN npm ci --only=production

COPY . .

ENV TOKEN=
ENV GUILD=
ENV CHANNEL=

CMD [ "node", "index.js" ]
