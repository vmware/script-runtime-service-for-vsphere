#!/bin/bash

# **************************************************************************$
#  Copyright 2020 VMware, Inc.$
#  SPDX-License-Identifier: Apache-2.0$
# **************************************************************************

PCLI_SOURCE_DIR=$1
IMAGE_DESTINATION_DIR=$2
SRS_VM_ADDRESS=$3

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
. $SCRIPT_DIR/constants.env

if [ -z "$PCLI_SOURCE_DIR" ];then
	echo -e "\e[31mERROR: Specify source PowerCLI Modules directory as first argument of this script.\e[0m"
	echo "Usage: build.sh <powershell modules dir> <image destination dir>"
	exit -1
fi

if [ -z "$IMAGE_DESTINATION_DIR" ];then
	echo -e "\e[31mERROR: Specify directory where image will be exported.\e[0m"
	echo "Usage: build.sh <powershell modules dir> <image destination dir>"
	exit -1
fi

SRC_DIR="$SCRIPT_DIR/../src"
PCLI_RUNSPACE_CONTAINER_BUILD="$SRC_DIR/Runspace/VMware.ScriptRuntimeService.RunspaceEndpoint/docker/pclirunspace/build.sh"

DOTNET_COMMAND="/usr/bin/dotnet"
DOCKER_COMMAND="/usr/bin/docker"

echo "INFO: Build PCLI Runspace Container '$PCLI_RUNSPACE_CONTAINER_BUILD $PCLI_SOURCE_DIR'"
$PCLI_RUNSPACE_CONTAINER_BUILD $PCLI_RUNSPACE_IMAGE_NAME $PCLI_SOURCE_DIR $DOTNET_COMMAND $DOCKER_COMMAND

if test -f "$IMAGE_DESTINATION_DIR/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE"; then
	rm -f "$IMAGE_DESTINATION_DIR/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE"
fi

echo "INFO: Export Containers to '$IMAGE_DESTINATION_DIR'"
$DOCKER_COMMAND save -o "$IMAGE_DESTINATION_DIR/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE" $PCLI_RUNSPACE_IMAGE_NAME

if [ -z "$SRS_VM_ADDRESS" ];then
	echo "INFO: SRS_VM_ADDRESS not spcified"
	echo "INFO: Success"
else
	echo "INFO: Copy image to SRS VM /root/ directory"
	scp "$IMAGE_DESTINATION_DIR/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE" "root@$SRS_VM_ADDRESS:/root/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE"
fi
