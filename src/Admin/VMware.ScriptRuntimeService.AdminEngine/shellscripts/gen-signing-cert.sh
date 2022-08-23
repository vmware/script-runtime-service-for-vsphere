#!/bin/bash

COMMON_NAME=$1
CERT_NAME=$2
OUTPUT_DIR=$3

SIGNING_CERTS_OUTPUT_DIR=$OUTPUT_DIR

OPENSSL_CLI=/usr/bin/openssl

echo "INFO: Generate signing certificate"

commonname=$COMMON_NAME
country=US
locality=California
state=CA
organization=VMware
organizationalunit=VMware Engineering

SIGN_CERT_NAME=$CERT_NAME
KEY_FILE=${SIGN_CERT_NAME}_key.pem
PEM_FILE=$SIGN_CERT_NAME.pem
SIGN_CERT_FILE=$SIGN_CERT_NAME.p12

$OPENSSL_CLI req -newkey rsa:2048 -nodes -keyout $SIGNING_CERTS_OUTPUT_DIR/$KEY_FILE -x509 -days 3650 -out $SIGNING_CERTS_OUTPUT_DIR/$PEM_FILE -subj "/C=$country/ST=$state/L=$locality/O=$organization/OU=$organizationalunit/CN=$commonname"
$OPENSSL_CLI x509 -text -noout -in $SIGNING_CERTS_OUTPUT_DIR/$PEM_FILE
$OPENSSL_CLI pkcs12 -inkey $SIGNING_CERTS_OUTPUT_DIR/$KEY_FILE -in $SIGNING_CERTS_OUTPUT_DIR/$PEM_FILE -export -out $SIGNING_CERTS_OUTPUT_DIR/$SIGN_CERT_FILE -passout pass:

rm -f $SIGNING_CERTS_OUTPUT_DIR/$KEY_FILE
rm -f $SIGNING_CERTS_OUTPUT_DIR/$PEM_FILE