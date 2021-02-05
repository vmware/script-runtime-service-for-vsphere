FROM ubuntu:18.04
RUN apt-get update && apt-get install -y autoconf automake curl cmake git libtool make \
    && git clone https://github.com/tsl0922/ttyd.git /ttyd \
    && cd /ttyd \
    && git reset --hard 3b174dac3b81072e7ac5259c844f300d5af2e955 \
    && env BUILD_TARGET=x86_64 WITH_SSL=true ./scripts/cross-build.sh

# Base layer image$
FROM srs-base:1.0

# Install Powershell Core
RUN tdnf install powershell -y

WORKDIR /app

# Copy application
COPY /app/service/ /app/service/
COPY /app/scripts/ /app/scripts/
COPY /app/PowerCLIModules/ /app/PowerCLIModules/

# Add PowerCLI Modules to PSModulePath
ENV PSModulePath="${PSModulePath}:/app/PowerCLIModules"

# Expose Runspace Endpoint on port 5550
ENV ASPNETCORE_URLS http://+:5550
EXPOSE 5550

COPY --from=0 /ttyd/build/ttyd /usr/bin/ttyd


ADD https://github.com/krallin/tini/releases/download/v0.18.0/tini /sbin/tini
RUN chmod +x /sbin/tini

ENTRYPOINT ["/sbin/tini", "--"]
