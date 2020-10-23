# **************************************************************************
#  Copyright 2020 VMware, Inc.
#  SPDX-License-Identifier: Apache-2.0
# **************************************************************************
#!/bin/bash

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

. $SCRIPT_DIR/constants.env

PCLI_SOURCE_DIR=$1
CONTAINERS_EXPORT_DIR=$2
DOTNET_COMMAND=$3
DOCKER_COMMAND=$4
PRODUCT_VERSION=$5
PRODUCT_VERSION_SUFFIX=$6

if [ -z "$PCLI_SOURCE_DIR" ];then
	echo -e "\e[31mERROR: Specify source PowerCLI Modules directory as first argument of this script.\e[0m"
	echo "Usage: buildcontainers.sh <powercli modules dir> <srs containters export dir>"$
	echo "Example: buildcontainers.sh ~/PowerCLIModules ~/output/srs-containers"
	exit -1
fi

if [ -z "$CONTAINERS_EXPORT_DIR" ];then
	echo -e "\e[31mERROR: Specify srs containers output directory.\e[0m"$
	echo "Usage: buildcontainers.sh <powercli modules dir> <srs containters export dir>"$
	echo "Example: buildcontainers.sh ~/PowerCLIModules ~/output/srs-containers"
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

if [ -z "$PRODUCT_VERSION_SUFFIX" ]; then
	PRODUCT_VERSION_SUFFIX="dev"
fi

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
SRC_DIR="$SCRIPT_DIR/../src"
BASE_LAYER_BUILD="$SRC_DIR/Runspace/VMware.ScriptRuntimeService.RunspaceEndpoint/docker/aspnetbaselayer/build.sh"
SRS_SETUP_CONTAINER_BUILD="$SRC_DIR/Admin/VMware.ScriptRuntimeService.Setup/docker/build.sh"
SRS_APIGATEWAY_CONTAINER_BUILD="$SRC_DIR/Endpoint/VMware.ScriptRuntimeService.APIGateway/docker/build.sh"
PCLI_RUNSPACE_CONTAINER_BUILD="$SRC_DIR/Runspace/VMware.ScriptRuntimeService.RunspaceEndpoint/docker/pclirunspace/build.sh"

echo "INFO: Build Base Layer Container '$BASE_LAYER_BUILD'"
$BASE_LAYER_BUILD $BASE_LAYER_IMAGE_NAME $DOTNET_COMMAND $DOCKER_COMMAND

echo "INFO: Build Srs Setup CLI Container '$SRS_SETUP_CONTAINER_BUILD'"
$SRS_SETUP_CONTAINER_BUILD $SETUP_IMAGE_NAME $DOTNET_COMMAND $DOCKER_COMMAND $CONTAINER_VERSION_LABEL

echo "INFO: Build API Gateway Container '$SRS_APIGATEWAY_CONTAINER_BUILD'"
$SRS_APIGATEWAY_CONTAINER_BUILD $APIGATEWAY_IMAGE_NAME $DOTNET_COMMAND $DOCKER_COMMAND $PRODUCT_VERSION $PRODUCT_VERSION_SUFFIX $CONTAINER_VERSION_LABEL

echo "INFO: Build PCLI Runspace Container '$PCLI_RUNSPACE_CONTAINER_BUILD $PCLI_SOURCE_DIR'"
$PCLI_RUNSPACE_CONTAINER_BUILD $PCLI_RUNSPACE_IMAGE_NAME $PCLI_SOURCE_DIR $DOTNET_COMMAND $DOCKER_COMMAND

echo "INFO: Cleanup '$CONTAINERS_EXPORT_DIR'"
if test -f "$CONTAINERS_EXPORT_DIR/$BASE_LAYER_IMAGE_NAME-$IMAGE_EXPORT_FILE"; then
	rm -f $CONTAINERS_EXPORT_DIR/$BASE_LAYER_IMAGE_NAME-$IMAGE_EXPORT_FILE
fi
if test -f "$CONTAINERS_EXPORT_DIR/$SETUP_IMAGE_NAME-$IMAGE_EXPORT_FILE"; then
	rm -f $CONTAINERS_EXPORT_DIR/$SETUP_IMAGE_NAME-$IMAGE_EXPORT_FILE
fi
if test -f "$CONTAINERS_EXPORT_DIR/$APIGATEWAY_IMAGE_NAME-$IMAGE_EXPORT_FILE"; then
	rm -f $CONTAINERS_EXPORT_DIR/$APIGATEWAY_IMAGE_NAME-$IMAGE_EXPORT_FILE
fi
if test -f "$CONTAINERS_EXPORT_DIR/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE$"; then
	rm -f $CONTAINERS_EXPORT_DIR/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE
fi

echo "INFO: Export Containers to '$CONTAINERS_EXPORT_DIR'"
$DOCKER_COMMAND save -o "$CONTAINERS_EXPORT_DIR/$BASE_LAYER_IMAGE_NAME-$IMAGE_EXPORT_FILE" $BASE_LAYER_IMAGE_NAME:$CONTAINER_VERSION_LABEL
$DOCKER_COMMAND save -o "$CONTAINERS_EXPORT_DIR/$SETUP_IMAGE_NAME-$IMAGE_EXPORT_FILE" $SETUP_IMAGE_NAME:$CONTAINER_VERSION_LABEL
$DOCKER_COMMAND save -o "$CONTAINERS_EXPORT_DIR/$APIGATEWAY_IMAGE_NAME-$IMAGE_EXPORT_FILE" $APIGATEWAY_IMAGE_NAME:$CONTAINER_VERSION_LABEL
$DOCKER_COMMAND save -o "$CONTAINERS_EXPORT_DIR/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE" $PCLI_RUNSPACE_IMAGE_NAME
