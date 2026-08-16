FROM alpine:latest

RUN apk add --no-cache \
    g++ \
    musl-dev

WORKDIR /workspace

RUN adduser -D -u 10001 judge

USER judge
