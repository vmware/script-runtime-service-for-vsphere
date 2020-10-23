#!/bin/bash -x

# **************************************************************************$
#  Copyright 2020 VMware, Inc.$
#  SPDX-License-Identifier: Apache-2.0$
# **************************************************************************
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

pushd $SCRIPT_DIR

echo "Building Script Runtime Service OVA Appliance ..."
rm -rf ./output-vmware-iso

echo "Applying packer build to photon.json ..."
packer build -var-file=photon-builder.json -var-file=photon-version.json photon.json

popd
