#!/bin/bash

IMAGE_NAME=$1
DOTNET_COMMAND=$2
DOCKER_COMMAND=$3
PRODUCT_VERSION=$4
PRODUCT_VERSION_SUFFIX=$5
IMAGE_VERSION=$6

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

APP_DESTINATION_DIR="$SCRIPT_DIR/app"
SERVICE_DESTINATION_DIR="$APP_DESTINATION_DIR/service"
SERVICE_SRC_DIR="$SCRIPT_DIR/../"

if [ -z "$IMAGE_NAME" ];then
	IMAGE_NAME="srs-admin-api"
fi

if [ -z "$DOTNET_COMMAND" ];then
	DOTNET_COMMAND="/usr/bin/dotnet"
fi

if [ -z "$DOCKER_COMMAND" ];then
	DOCKER_COMMAND="/usr/bin/docker"
fi

if [ -z "$PRODUCT_VERSION" ];then
	PRODUCT_VERSION="1.0.0"
fi

if [ -z "$IMAGE_VERSION" ];then
	IMAGE_VERSION=1.0
fi

echo "INFO: Service Sources Dir is: $SERVICE_SRC_DIR"
echo "INFO: App destination dir is: $APP_DESTINATION_DIR"
echo "INFO: Service Destination Dir is: $SERVICE_DESTINATION_DIR"

echo "INFO: Creating direcctory $APP_DESTINATION_DIR"
mkdir $APP_DESTINATION_DIR
echo "INFO: Creating directory $SERVICE_DESTINATION_DIR"
mkdir $SERVICE_DESTINATION_DIR

echo "INFO: dotnet publish $SERVICE_SRC_DIR"
# The command below publishes all the dependencies in the output folder
$DOTNET_COMMAND publish $SERVICE_SRC_DIR -c Release -f netcoreapp3.1 -o $SERVICE_DESTINATION_DIR
echo "INFO: dotnet build with Product Version: $PRODUCT_VERSION, Version Suffix: $PRODUCT_VERSION_SUFFIX"
$DOTNET_COMMAND build $SERVICE_SRC_DIR -c Release -f netcoreapp3.1 -r linux-x64 -p:Version="$PRODUCT_VERSION-$PRODUCT_VERSION_SUFFIX" -o $SERVICE_DESTINATION_DIR

echo "INFO: Building $IMAGE_NAME image"
cd $SCRIPT_DIR
$DOCKER_COMMAND build -t $IMAGE_NAME:$IMAGE_VERSION .

echo "INFO: Removing $APP_DESTINATION_DIR"
rm -rf $APP_DESTINATION_DIR
