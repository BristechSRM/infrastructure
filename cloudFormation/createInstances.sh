#!/bin/sh -eu

if [ "$#" -ne 3 ]; then
    echo "Usage: $0 environment-tag subnet-id groupset-id"
    exit 1
fi

ENVIRONMENT_TAG=$1
SUBNET_ID=$2
GROUPSET_ID=$3

aws cloudformation create-stack --stack-name i-all --template-body file://i-all.json --parameters \
    ParameterKey=environment,ParameterValue=${ENVIRONMENT_TAG} \
    ParameterKey=subnet,ParameterValue=${SUBNET_ID} \
    ParameterKey=groupSet,ParameterValue=${GROUPSET_ID} \
    ParameterKey=authEIP,ParameterValue=52.51.54.255 \
    ParameterKey=apiEIP,ParameterValue=52.50.88.117 \
    ParameterKey=frontendEIP,ParameterValue=52.50.27.136
