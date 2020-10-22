# Base layer image
FROM srs-base:1.0

# Install zlib-dvel needed for setup TLS communication
RUN tdnf install zlib-devel -y

# Copy API Gateway web service application
ADD /app/service/ /app/service/

WORKDIR /app/service

# Expose API Gateway Endpoint on port 5050
ENV ASPNETCORE_URLS http://+:5050
EXPOSE 5050

# Start SES API Gateway Endpoint Service
CMD ["/app/service/VMware.ScriptRuntimeService.APIGateway"]
#CMD ["/bin/bash"]
