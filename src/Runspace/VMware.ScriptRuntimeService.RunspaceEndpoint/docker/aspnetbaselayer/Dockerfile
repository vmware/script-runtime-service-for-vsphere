# Base OS is Photon 3.0$
#FROM photon:3.0
FROM photonos-docker-local.artifactory.eng.vmware.com/photon3:20190823

# Install gcc reqquired for .NET Core
RUN tdnf install gcc -y

WORKDIR /app/service

# Copy build output to /app/service application
COPY /app/service/ /app/service/
