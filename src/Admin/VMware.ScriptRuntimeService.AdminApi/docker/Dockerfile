# Base layer image
FROM srs-base:1.0

# Install zlib-dvel needed for setup TLS communication
RUN tdnf install zlib-devel -y

# Copy SRS setup application
ADD /app/service/ /app/service/

WORKDIR /app/service

# Expose API Gateway Endpoint on port 5480
ENV ASPNETCORE_URLS http://+:5555
EXPOSE 5555

# Start SES API Gateway Endpoint Service
CMD ["/app/service/VMware.ScriptRuntimeService.AdminApi"]
