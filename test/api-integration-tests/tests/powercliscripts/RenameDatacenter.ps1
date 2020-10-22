# **************************************************************************
#  Copyright 2020 VMware, Inc.
#  SPDX-License-Identifier: Apache-2.0
# **************************************************************************

# Test script for the purpos of SES integration tests
# Renames Datacenter given on the datacenter parameter
# with name given on name parameter
param($datacenter, $name)

Set-Datacenter -Datacenter $datacenter -Name $name