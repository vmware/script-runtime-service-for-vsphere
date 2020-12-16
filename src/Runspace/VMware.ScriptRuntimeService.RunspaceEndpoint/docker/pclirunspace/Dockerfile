# Base layer image$
FROM srs-base:1.0

# Install Powershell Core
RUN tdnf install powershell -y

WORKDIR /app

# Copy application
COPY /app/service/ /app/service/
COPY /app/PowerCLIModules/ /app/PowerCLIModules/
COPY /app/ttyd/ /app/ttyd/
RUN chmod +x /app/ttyd/ttyd

# Add PowerCLI Modules to PSModulePath
ENV PSModulePath="${PSModulePath}:/app/PowerCLIModules"

# Expose Runspace Endpoint on port 5550
ENV ASPNETCORE_URLS http://+:5550
EXPOSE 5550

# Start Runspace Endpoint Service
ENTRYPOINT ["/app/service/VMware.ScriptRuntimeService.RunspaceEndpoint"]
