FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine

WORKDIR /workspace

RUN addgroup --system judge && \
    adduser --system --ingroup judge judge

USER judge
