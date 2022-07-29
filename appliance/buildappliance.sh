#!/bin/bash -x

# **************************************************************************$
#  Copyright 2020 VMware, Inc.$
#  SPDX-License-Identifier: Apache-2.0$
# **************************************************************************
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

PACKER_BUILDER_TYPE="${1,,}"

pushd $SCRIPT_DIR

echo "Building Script Runtime Service OVA Appliance ..."
rm -rf ./output-vmware-iso


if [ "$PACKER_BUILDER_TYPE" = "vsphere" ];then
	echo "Applying packer build to photon-vsphere.json ..."
	packer build -var-file=photon-builder-vsphere.json -var-file=photon-version.json photon-vsphere.json
else
	echo "Applying packer build to photon.json ..."
	packer build -var-file=photon-builder.json -var-file=photon-version.json photon.json
fi

popd
