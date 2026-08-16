FROM node:26-alpine

WORKDIR /workspace

RUN npm install -g typescript

RUN addgroup -S judge && \
    adduser -S judge -G judge

USER judge
