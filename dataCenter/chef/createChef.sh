#!/bin/sh -eu

if [ "$#" -ne 3 ]; then
    echo "Usage: $0 environment-tag vpc-id subnet-id"
    exit 1
fi

ENVIRONMENT_TAG=$1
VPC_ID=$2
SUBNET_ID=$3

aws cloudformation create-stack --stack-name ${ENVIRONMENT_TAG}-srm-chef --template-body file://srm-chef.json --parameters \
    ParameterKey=environment,ParameterValue=${ENVIRONMENT_TAG} \
    ParameterKey=vpcId,ParameterValue=${VPC_ID} \
    ParameterKey=servicesSubnetId,ParameterValue=${SUBNET_ID}
