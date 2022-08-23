#!/bin/bash

COMMON_NAME=$1
CERT_NAME=$2
OUTPUT_DIR=$3

SIGNING_CERTS_OUTPUT_DIR=$OUTPUT_DIR

OPENSSL_CLI=/usr/bin/openssl

echo "INFO: Generate tls certificate"

commonname=$COMMON_NAME
country=US
locality=California
state=CA
organization=VMware
organizationalunit=VMware Engineering

$OPENSSL_CLI req -x509 -nodes -days 365 -newkey rsa:2048 -keyout $OUTPUT_DIR/${CERT_NAME}.key -out $OUTPUT_DIR/${CERT_NAME}.crt -subj "/C=$country/ST=$state/L=$locality/O=$organization/OU=$organizationalunit/CN=$commonname"