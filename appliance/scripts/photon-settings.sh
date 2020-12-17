#!/bin/bash -eux

##
## Misc configuration
##

echo '> Disable IPv6'
echo "net.ipv6.conf.all.disable_ipv6 = 1" >> /etc/sysctl.conf

pushd /etc/yum.repos.d/
sed  -i 's/dl.bintray.com\/vmware/packages.vmware.com\/photon\/$releasever/g' photon.repo photon-updates.repo photon-extras.repo photon-debuginfo.repo
popd

echo '> Applying latest Updates...'
tdnf -y update

echo '> Installing Additional Packages...'
tdnf install -y \
  less \
  logrotate \
  curl \
  wget \
  unzip \
  awk \
  tar

echo '> Done'
