#!/bin/bash

# **************************************************************************$
#  Copyright 2020 VMware, Inc.$
#  SPDX-License-Identifier: Apache-2.0$
# **************************************************************************

# Bootstrap script

set -euo pipefail

if [ -e /root/ran_customization ]; then
    exit
else
    NETWORK_CONFIG_FILE=$(ls /etc/systemd/network | grep .network)

    DEBUG_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "guestinfo.debug")
    DEBUG=$(echo "${DEBUG_PROPERTY}" | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    LOG_FILE=/var/log/bootstrap.log
    if [ ${DEBUG} == "True" ]; then
        LOG_FILE=/var/log/photon-customization-debug.log
        echo "DEBUG BOOTSTRAP" > /dev/console
        set -x
        exec 2> ${LOG_FILE}
        echo
        echo "### WARNING -- DEBUG LOG CONTAINS ALL EXECUTED COMMANDS WHICH INCLUDES CREDENTIALS -- WARNING ###"
        echo "### WARNING --             PLEASE REMOVE CREDENTIALS BEFORE SHARING LOG            -- WARNING ###"
        echo
    fi

    HOSTNAME_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "guestinfo.hostname")
    IP_ADDRESS_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "guestinfo.ipaddress")
    NETMASK_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "guestinfo.netmask")
    GATEWAY_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "guestinfo.gateway")
    DNS_SERVER_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "guestinfo.dns")
    DNS_DOMAIN_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "guestinfo.domain")
    ROOT_PASSWORD_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "guestinfo.root_password")

    ##################################
    ### No User Input, assume DHCP ###
    ##################################
    IP_ADDRESS=$(echo $IP_ADDRESS_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    HOSTNAME=$(echo "${HOSTNAME_PROPERTY}" | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    if [ -z ${IP_ADDRESS} ]; then
        cat > /etc/systemd/network/${NETWORK_CONFIG_FILE} << __CUSTOMIZE_PHOTON__
[Match]
Name=e*

[Network]
DHCP=yes
IPv6AcceptRA=no
__CUSTOMIZE_PHOTON__
    #########################
    ### Static IP Address ###
    #########################
    else
        NETMASK=$(echo "${NETMASK_PROPERTY}" | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
        GATEWAY=$(echo "${GATEWAY_PROPERTY}" | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
        DNS_SERVER=$(echo "${DNS_SERVER_PROPERTY}" | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
        DNS_DOMAIN=$(echo "${DNS_DOMAIN_PROPERTY}" | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')

        echo -e "\e[92mConfiguring Static IP Address ..." > /dev/console
        cat > /etc/systemd/network/${NETWORK_CONFIG_FILE} << __CUSTOMIZE_PHOTON__
[Match]
Name=e*
w

[Network]
Address=${IP_ADDRESS}/${NETMASK}
Gateway=${GATEWAY}
DNS=${DNS_SERVER}
Domain=${DNS_DOMAIN}
__CUSTOMIZE_PHOTON__

       echo -e "\e[92mConfiguring hostname ..." > /dev/console
       hostnamectl set-hostname ${HOSTNAME}
       echo "${IP_ADDRESS} ${HOSTNAME}" >> /etc/hosts
       echo -e "\e[92mRestarting Network ..." > /dev/console
       systemctl restart systemd-networkd
    fi

    if [ -z ${HOSTNAME} ]; then
      echo "Hostname not specified"
    else
       echo -e "\e[92mConfiguring hostname '${HOSTNAME}' ..." > /dev/console
       hostnamectl set-hostname ${HOSTNAME}
    fi

    ROOT_PASSWORD=$(echo "${ROOT_PASSWORD_PROPERTY}" | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    if [ -z ${ROOT_PASSWORD} ]; then
      echo 'No root password is specified' > /dev/console
    else
      echo -e "\e[92mConfiguring root password ..." > /dev/console
      echo "root:${ROOT_PASSWORD}" | /usr/sbin/chpasswd
    fi

    echo "Step 1: Setup K8s Cluster" > /dev/console
    echo "Enable docker" > /dev/console
    systemctl enable docker
    systemctl start docker

    DOCKER_USER_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "srs.dockeruser")
    DOCKER_USER=$(echo $DOCKER_USER_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    DOCKER_PASS_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "srs.dockerpassword")
    DOCKER_PASS=$(echo $DOCKER_PASS_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')

    if [ -z ${DOCKER_USER} ]; then
      echo "Docker user not specified"
    else
      echo "Using Docker user ${DOCKER_USER}"
      docker login --username=$DOCKER_USER --password=$DOCKER_PASS
    fi

    echo "Create Cluster with ingress ready. Configures host port forwarding to ingress" > /dev/console
    cat <<EOF | kind create cluster --config=-
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
- role: control-plane
  kubeadmConfigPatches:
  - |
    kind: InitConfiguration
    nodeRegistration:
      kubeletExtraArgs:
        node-labels: "ingress-ready=true"
  extraPortMappings:
  - containerPort: 443
    hostPort: 443
    protocol: TCP
  - containerPort: 6443
    hostPort: 6443
    protocol: TCP
EOF
     echo "Pull nginx docker images"
     docker pull docker.io/jettech/kube-webhook-certgen:v1.2.2
     docker pull us.gcr.io/k8s-artifacts-prod/ingress-nginx/controller:v0.34.1

     echo "Load nginx docker image to kind node"
     kind load docker-image jettech/kube-webhook-certgen:v1.2.2
     kind load docker-image us.gcr.io/k8s-artifacts-prod/ingress-nginx/controller:v0.34.1

     echo "Deploy ingress controller for SRS on K8s Cluster" > /dev/console
     kubectl apply -f /root/ingress-controller.yaml

     FILE=/root/.kube/config
     if ! [[ -f $FILE ]]; then
        echo "Export kubeconfig" > /dev/console
        mkdir /root/.kube
        kind get kubeconfig > /root/.kube/config
     fi

    echo "Wait ingress controller to become ready" > /dev/console
    INGRESS_CONTROLLER_POD=$(kubectl -n ingress-nginx get pod -l app.kubernetes.io/component=controller)
    RETRY_COUNT=0
    while [[ -z "$INGRESS_CONTROLLER_POD" ]] && [[ $RETRY_COUNT -lt 10  ]]
    do
       sleep 10
       INGRESS_CONTROLLER_POD=$(kubectl -n ingress-nginx get pod -l app.kubernetes.io/component=controller)
       let "RETRY_COUNT+=1"
       echo "DEBUG: POD Found '$INGRESS_CONTROLLER_POD', Retry count '$RETRY_COUNT'" > /dev/console
    done

    echo "DEBUG: Waiting for ingress controller pod complete" > /dev/console
    if [[ $RETRY_COUNT == 10 ]]; then
       echo -e "\e[31mERROR: Wating for ingress controller timed out. Check ingress status with 'kubectl -n ingress-nginx get pod'" > /dev/console
    else
       echo "DEBUG: Start waiting controller pod to become 'Running'" > /dev/console
       INGRESS_CONTROLLER_STATUS=$(kubectl -n ingress-nginx get pod -l app.kubernetes.io/component=controller -o jsonpath={.items[0].status.phase})
       echo "Ingress Controller Status '${INGRESS_CONTROLLER_STATUS}', Retry count '$RETRY_COUNT'" > /dev/console
       RETRY_COUNT=0

       while [[ $INGRESS_CONTROLLER_STATUS != "Running" ]] && [[ $RETRY_COUNT -lt 10 ]]
       do

          sleep 10
          let "RETRY_COUNT+=1"
          INGRESS_CONTROLLER_STATUS=$(kubectl -n ingress-nginx get pod -l app.kubernetes.io/component=controller -o jsonpath={.items[0].status.phase})
          echo "DEBUG: Ingress Controller Status '${INGRESS_CONTROLLER_STATUS}', Retry count '$RETRY_COUNT'" > /dev/console
       done

       if [[ $RETRY_COUNT == 10 ]]; then
          echo -e "\e[31mERROR: Wating for ingress controller timed out. Check ingress status with 'kubectl -n ingress-nginx get pod'" > /dev/console$
       fi
    fi

    echo -e "\e[92mStep 2: Pull images from local store" > /dev/console
    echo -e "\e[92mLoad srs docker images in docker" > /dev/console
    cat *.tar | docker load # issue the load command to try and load all images at once

    echo -e "\e[92mPre-pull srs docker images in kind k8s node" > /dev/console
    SRS_IMAGES_VERSION=1.0
    kind load docker-image srs-setup:$SRS_IMAGES_VERSION
    kind load docker-image srs-adminapi:$SRS_IMAGES_VERSION
    kind load docker-image srs-apigateway:$SRS_IMAGES_VERSION
    kind load docker-image pclirunspace:latest

    echo -e "\e[92mCleanup image files" > /dev/console
    rm -rf /root/*.tar

    echo -e "\e[92mStep 3: Deploy SRS" > /dev/console
    echo -e "\e[92mEdit srs-app.yaml with VC details" > /dev/console
    VC_ADDRESS_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "srs.vcaddress")
    VC_IP=$(echo $VC_ADDRESS_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    VC_USER_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "srs.vcuser")
    VC_USER=$(echo $VC_USER_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    VC_PASSWORD_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "srs.vcpassword")
    VC_PASSWORD=$(echo $VC_PASSWORD_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    VC_THUMBPRINT_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "srs.vcthumbprint")
    VC_TLS_THUMBPRINT=$(echo $VC_THUMBPRINT_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    ADMIN_USER_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "srs.adminuser")
    ADMIN_USER=$(echo $ADMIN_USER_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}' | base64)
    ADMIN_PASSWORD_PROPERTY=$(vmtoolsd --cmd "info-get guestinfo.ovfEnv" | grep "srs.adminpassword")
    ADMIN_PASSWORD=$(echo $ADMIN_PASSWORD_PROPERTY | awk -F 'oe:value="' '{print $2}' | awk -F '"' '{print $1}')
    SRSA_HOSTNAME=""
    if [ -z ${HOSTNAME} ]; then
      echo "Hostname not specified, setting default"
      SRSA_HOSTNAME="srsa"
    else
       SRSA_HOSTNAME=$HOSTNAME
    fi
    ADMIN_PASSWORD_SALT=$(openssl rand -base64 12)
    ADMIN_PASSWORD=$(echo -n "$ADMIN_PASSWORD_SALT$ADMIN_PASSWORD" | sha256sum | cut -d ' ' -f 1 | base64 | tr -d '\n')
    ADMIN_PASSWORD_SALT=$(echo -n "$ADMIN_PASSWORD_SALT" | base64 | tr -d '\n')

    sed -e "s/\${VC_SERVER}/$VC_IP/" -e "s/\${VC_USER}/$VC_USER/" -e "s/\${VC_PASSWORD}/$VC_PASSWORD/" -e "s/\${VC_THUMBPRINT}/$VC_TLS_THUMBPRINT/" -e "s/\${ADMIN_USER}/$ADMIN_USER/" -e "s/\${ADMIN_PASSWORD}/$ADMIN_PASSWORD/" -e "s/\${ADMIN_PASSWORD_SALT}/$ADMIN_PASSWORD_SALT/" -e "s/\${SRSA_HOSTNAME}/$SRSA_HOSTNAME/" /root/srs-app-template.yaml > /root/srs-app.yaml

    echo -e "\e[92mDeploy SRS on K8s Cluster" > /dev/console
    kubectl apply -f /root/srs-app.yaml

    echo -e"\e[92mWait srs-setup status to become running" > /dev/console
    SRS_APIGATEWAY_STATUS=$(kubectl -n script-runtime-service get pod -l app=srs-apigateway -o jsonpath={.items[0].status.phase})
    SRS_ADMINAPI_STATUS=$(kubectl -n script-runtime-service get pod -l app=srs-adminapi -o jsonpath={.items[0].status.phase})
    RETRY_COUNT=0

    while [[ $SRS_APIGATEWAY_STATUS != "Running" ]] && [[ $SRS_ADMINAPI_STATUS != "Running" ]] && [[ $RETRY_COUNT -lt 10 ]]
    do
        sleep 10
        let "RETRY_COUNT+=1"
        SRS_SETUP_STATUS=$(kubectl -n script-runtime-service get pod -l app=srs-apigateway -o jsonpath={.items[0].status.phase})
        SRS_ADMINAPI_STATUS=$(kubectl -n script-runtime-service get pod -l app=srs-adminapi -o jsonpath={.items[0].status.phase})
        echo "DEBUG: Srs ApiGateway Status '${SRS_APIGATEWAY_STATUS}',  Srs AdminApi Status '${SRS_ADMINAPI_STATUS}', Retry count '$RETRY_COUNT'" > /dev/console
    done

    # Ensure we don't run customization again
    touch /root/ran_customization
fi
