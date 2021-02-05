#!/bin/bash

IMAGE_NAME=$1
PCLI_SOURCE_DIR=$2
DOTNET_COMMAND=$3
DOCKER_COMMAND=$4

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

if [ -z "$IMAGE_NAME" ];then
    echo -e "\033[0;31mERROR: Container image name not specified\033[0m"$
    echo "Usage: build.sh <container image name> <PowerCLI Modules Directory>"$
    exit 1$
fi

if [ -z "$PCLI_SOURCE_DIR" ];then
   echo -e "\033[0;31mERROR: PowerCLI Modules Source directory is empty\033[0m"
   echo "Usage: build.sh <PowerCLI Modules Directory>"
   exit 1
fi

if [ -z "$DOTNET_COMMAND" ];then
	DOTNET_COMMAND="/usr/bin/dotnet"
fi

if [ -z "$DOCKER_COMMAND" ];then
	DOCKER_COMMAND="/usr/bin/docker"
fi

echo "INFO: Working directory is: $SCRIPT_DIR"
echo "INFO: PowerCLI directory is: $PCLI_SOURCE_DIR"


APP_DESTINATION_DIR="$SCRIPT_DIR/app"
PCLI_DESTINATION_DIR="$APP_DESTINATION_DIR/PowerCLIModules"
SERVICE_DESTINATION_DIR="$APP_DESTINATION_DIR/service"
APP_SCRIPTS_DIR="$APP_DESTINATION_DIR/scripts"
SERVICE_SRC_DIR="$SCRIPT_DIR/../../"

echo "INFO: PowerCLI Destination dir is: $PCLI_DESTINATION_DIR"
echo "INFO: Service Sources dir is: $SERVICE_SRC_DIR"
echo "INFO: App destination dir is: $APP_DESTINATION_DIR"
echo "INFO: Service Destination dir is: $SERVICE_DESTINATION_DIR"
echo "INFO: Ttyd Destination dir is: $TTYD_DESTINATION_DIR"

echo "INFO: Creating direcctory $APP_DESTINATION_DIR"
mkdir $APP_DESTINATION_DIR
echo "INFO: Creating directory $SERVICE_DESTINATION_DIR"
mkdir $SERVICE_DESTINATION_DIR
echo "INFO: Creating directory $APP_SCRIPTS_DIR"
mkdir $APP_SCRIPTS_DIR
echo "INFO: Copy connect script to $APP_SCRIPTS_DIR"
cp "$SCRIPT_DIR/connect.ps1" "$APP_SCRIPTS_DIR/connect.ps1"

echo "INFO: dotnet publish $SERVICE_SRC_DIR"
$DOTNET_COMMAND publish $SERVICE_SRC_DIR -c Release -f netcoreapp3.1 -o $SERVICE_DESTINATION_DIR
$DOTNET_COMMAND build $SERVICE_SRC_DIR -c Release -f netcoreapp3.1 -r linux-x64 -o $SERVICE_DESTINATION_DIR

echo "INFO: Creating directory $PCLI_DESTINATION_DIR"
mkdir $PCLI_DESTINATION_DIR

echo "INFO: Copying PowerCLI Modules from  $PCLI_SOURCE_DIR to $PCLI_DESTINATION_DIR"
cp -r $PCLI_SOURCE_DIR/* $PCLI_DESTINATION_DIR

echo "INFO: Building $IMAGE_NAME image"
cd $SCRIPT_DIR
$DOCKER_COMMAND build -t $IMAGE_NAME .

echo "INFO: Removing $APP_DESTINATION_DIR"
rm -rf $APP_DESTINATION_DIR
