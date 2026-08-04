FROM alpine:latest

RUN apk add --no-cache g++ musl-dev

WORKDIR /workspace

COPY run.sh /run.sh
RUN chmod +x /run.sh

ENTRYPOINT ["/run.sh"]
