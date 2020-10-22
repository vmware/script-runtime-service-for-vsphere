# **************************************************************************
#  Copyright 2020 VMware, Inc.
#  SPDX-License-Identifier: Apache-2.0
# **************************************************************************

# Test script for the purpos of SES integration tests
# Creates 10 Datacenters, Updates datacenters names, Gets datacenters, and Removes Datacenters
param()

$datacenterLocation = 'datacenters'
$datacenterBaseName = 'SES-Test-Datacenter'

# Create datacenters
for ($i = 0; $i -lt 10; $i++) {
   New-Datacenter -Name "$datacenterBaseName-$i" -Location $datacenterLocation
}

# Get and Update Datacenters' Names
for ($i = 0; $i -lt 10; $i++) {
   Get-Datacenter -Name "$datacenterBaseName-$i" | Set-Datacenter -Name "$datacenterBaseName-$i-updated"
}

# Remove created datacenters
Get-Datacenter -Name "$datacenterBaseName-*" | Remove-Datacenter -Confirm:$false