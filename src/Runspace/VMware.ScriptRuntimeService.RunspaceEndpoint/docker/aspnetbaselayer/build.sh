#!/bin/bash

IMAGE_NAME=$1
DOTNET_COMMAND=$2
DOCKER_COMMAND=$3

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

APP_DESTINATION_DIR="$SCRIPT_DIR/app"
SERVICE_DESTINATION_DIR="$APP_DESTINATION_DIR/service"
SERVICE_SRC_DIR="$SCRIPT_DIR/../../"

 if [ -z "$IMAGE_NAME" ];then
	IAMGE_NAME="srs-base"
fi
if [ -z "$DOTNET_COMMAND" ];then
	DOTNET_COMMAND="/usr/bin/dotnet"
fi

if [ -z "$DOCKER_COMMAND" ];then
	DOCKER_COMMAND="/usr/bin/docker"
fi

echo "INFO: Service Sources Dir is: $SERVICE_SRC_DIR"
echo "INFO: App destination dir is: $APP_DESTINATION_DIR"
echo "INFO: Service Destination Dir is: $SERVICE_DESTINATION_DIR"

echo "INFO: Creating direcctory $APP_DESTINATION_DIR"
mkdir $APP_DESTINATION_DIR
echo "INFO: Creating directory $SERVICE_DESTINATION_DIR"
mkdir $SERVICE_DESTINATION_DIR

echo "INFO: dotnet publish $SERVICE_SRC_DIR"
$DOTNET_COMMAND publish $SERVICE_SRC_DIR -c Release -f netcoreapp3.1 -r linux-x64 -o $SERVICE_DESTINATION_DIR
echo "INFO: Removing build artificats since they are not needed for base layer. Base layer should container only ASP.NET core redistributable"
rm -rf $SERVICE_DESTINATION_DIR/VMware.ScriptRuntimeService*

echo "INFO: Building srs-base image"
cd $SCRIPT_DIR
$DOCKER_COMMAND build -t $IMAGE_NAME:1.0 .

echo "INFO: Removing $APP_DESTINATION_DIR"
rm -rf $APP_DESTINATION_DIR
