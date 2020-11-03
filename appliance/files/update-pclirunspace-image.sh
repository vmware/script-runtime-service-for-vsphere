#!/bin/bash

# **************************************************************************$
#  Copyright 2020 VMware, Inc.$
#  SPDX-License-Identifier: Apache-2.0$
# **************************************************************************

docker load < /root/pclirunspace-docker-image.tar
kind load docker-image pclirunspace:latest
