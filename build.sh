#!/bin/bash -x

# **************************************************************************$
#  Copyright 2020 VMware, Inc.$
#  SPDX-License-Identifier: Apache-2.0$
# **************************************************************************

PCLI_SOURCE_DIR=$1

PACKER_BUILDER_TYPE="$2" | tr '[:upper:]' '[:lower:]' # second argument to lower case

if [ -z "$PCLI_SOURCE_DIR" ];then
	echo -e "\e[31mERROR: Specify source PowerCLI Modules directory as first argument of this script.\e[0m"
	echo "Usage: build.sh ~/PowerCLIMoudles"
	exit -1
fi

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

BUILD_CONTAINERS_SCRIPT="$SCRIPT_DIR/build-scripts/buildcontainers.sh"
BUILD_APPLIANCE_SCRIPT="$SCRIPT_DIR/appliance/buildappliance.sh"

CONTAINERS_OUTPUT_DIR="$SCRIPT_DIR/appliance/files"

echo "[Step 1] Build SRS containers"
$BUILD_CONTAINERS_SCRIPT $PCLI_SOURCE_DIR $CONTAINERS_OUTPUT_DIR

echo "[Step 2] Build SRS appliance"
if [ "$PACKER_BUILDER_TYPE" = "vsphere" ];then
	$BUILD_APPLIANCE_SCRIPT "vsphere"
else
	$BUILD_APPLIANCE_SCRIPT
fi

