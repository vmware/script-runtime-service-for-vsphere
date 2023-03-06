# Base layer image$
FROM srs-base:1.0

# Install Powershell Core
RUN tdnf install powershell -y

RUN tdnf install python3-3.7.5 python3-xml -y
RUN python3 -m ensurepip
RUN pip3 install six psutil lxml pyopenssl

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

ADD https://github.com/tsl0922/ttyd/releases/download/1.7.3/ttyd.x86_64 /usr/bin/ttyd
RUN chmod +x /usr/bin/ttyd

ADD https://github.com/krallin/tini/releases/download/v0.18.0/tini /sbin/tini
RUN chmod +x /sbin/tini

ENTRYPOINT ["/sbin/tini", "--"]
