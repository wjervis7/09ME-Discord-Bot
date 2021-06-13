FROM node

WORKDIR /usr/src/app

COPY package*.json ./

RUN npm ci --only=production

COPY . .

ENV TOKEN=
ENV GUILD=
ENV CHANNEL=
ENV DB_NAME=
ENV DB_USER=
ENV DB_PASS=
ENV DB_HOST=
ENV DB_PORT=

CMD [ "node", "app.js" ]
