#!/bin/bash

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

. $SCRIPT_DIR/constants.env

PCLI_SOURCE_DIR=$1

SRS_HOST=$2
SRS_HOST_USER=$3
SRS_HOST_PASSWORD=$4

function print_error() {
	echo -e "\e[31mERROR: $1 \e[0m"
}

function print_info() {
	echo -e "INFO: $1"
}

function print_usage() {
	echo "Usage: buildanddeploy.sh <powercli modules dir> <srs host address> <srs host ssh user> <ses host ssh password>"
	echo "Example: buildanddeploy.sh ~/PowerCLIModules 10.23.82.191 root 'passW0rd'"
}

if [ -z "$SRS_HOST" ];then
	print_error "Specify SRS HOST Address, User, and Password to deploy."
	print_usage
	exit -1
fi

if [ -z "$SRS_HOST_USER" ];then
	print_error "Specify SRS HOST Address, User, and Password to deploy."
	print_usage
	exit -1
fi

if [ -z "$SRS_HOST_PASSWORD" ];then
	print_error "Specify SRS HOST Address, User, and Password to deploy."
	print_usage
	exit -1
fi

CONTAINERS_EXPORT_DIR=$SCRIPT_DIR/tmpoutput

mkdir $CONTAINERS_EXPORT_DIR
print_info "Build containers"
$SCRIPT_DIR/buildcontainers.sh $PCLI_SOURCE_DIR $CONTAINERS_EXPORT_DIR

print_info "Copy images to SRS HOST"
sshpass -p $SRS_HOST_PASSWORD scp "$CONTAINERS_EXPORT_DIR/$APIGATEWAY_IMAGE_NAME-$IMAGE_EXPORT_FILE" "$SRS_HOST_USER@$SRS_HOST:/$SRS_HOST_USER/$APIGATEWAY_IMAGE_NAME-$IMAGE_EXPORT_FILE"
sshpass -p $SRS_HOST_PASSWORD scp "$CONTAINERS_EXPORT_DIR/$SETUP_IMAGE_NAME-$IMAGE_EXPORT_FILE" "$SRS_HOST_USER@$SRS_HOST:/$SRS_HOST_USER/$SETUP_IMAGE_NAME-$IMAGE_EXPORT_FILE"
sshpass -p $SRS_HOST_PASSWORD scp "$CONTAINERS_EXPORT_DIR/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE" "$SRS_HOST_USER@$SRS_HOST:/$SRS_HOST_USER/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE"

print_info "Remove previous images from kind control plane"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker exec kind-control-plane crictl rmi docker.io/library/$APIGATEWAY_IMAGE_NAME:$CONTAINER_VERSION_LABEL"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker exec kind-control-plane crictl rmi docker.io/library/$SETUP_IMAGE_NAME:$CONTAINER_VERSION_LABEL"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker exec kind-control-plane crictl rmi docker.io/library/$PCLI_RUNSPACE_IMAGE_NAME:$CONTAINER_VERSION_LABEL"

print_info "Load images on SRS HOST Kind K8s Cluster"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker load < /$SRS_HOST_USER/$APIGATEWAY_IMAGE_NAME-$IMAGE_EXPORT_FILE"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "kind load docker-image $APIGATEWAY_IMAGE_NAME:$CONTAINER_VERSION_LABEL"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker image rm $APIGATEWAY_IMAGE_NAME:$CONTAINER_VERSION_LABEL --force"

sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker load < /$SRS_HOST_USER/$SETUP_IMAGE_NAME-$IMAGE_EXPORT_FILE"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "kind load docker-image $SETUP_IMAGE_NAME:$CONTAINER_VERSION_LABEL"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker image rm $SETUP_IMAGE_NAME:$CONTAINER_VERSION_LABEL --force"

sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker load < /$SRS_HOST_USER/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "kind load docker-image $PCLI_RUNSPACE_IMAGE_NAME:latest"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "docker image rm $PCLI_RUNSPACE_IMAGE_NAME:latest --force"

print_info "Remove image files from SRS HOST"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "rm /$SRS_HOST_USER/$APIGATEWAY_IMAGE_NAME-$IMAGE_EXPORT_FILE"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "rm /$SRS_HOST_USER/$SETUP_IMAGE_NAME-$IMAGE_EXPORT_FILE"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "rm /$SRS_HOST_USER/$PCLI_RUNSPACE_IMAGE_NAME-$IMAGE_EXPORT_FILE"

print_info "Remove docker containers from local machine"
docker image rm $APIGATEWAY_IMAGE_NAME:$CONTAINER_VERSION_LABEL --force
docker image rm $SETUP_IMAGE_NAME:$CONTAINER_VERSION_LABEL --force
docker image rm $PCLI_RUNSPACE_IMAGE_NAME --force
docker image rm $BASE_LAYER_IMAGE_NAME:$CONTAINER_VERSION_LABEL --force

print_info "Delete temp output directory"
rm -rf $CONTAINERS_EXPORT_DIR

print_info "Restart SRS Service on SRS HOST"
sshpass -p $SRS_HOST_PASSWORD ssh $SRS_HOST_USER@$SRS_HOST "kubectl -n script-runtime-service delete pod \$(kubectl -n script-runtime-service get pod -l app=srs-apigateway -o jsonpath={.items[0].metadata.name})"
