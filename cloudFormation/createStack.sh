#!/bin/sh -eu

if [ "$#" -ne 6 ]; then
    echo "Usage: $0 environment-tag vpc-id subnet-id authEIP apiEIP frontendEIP"
    exit 1
fi

ENVIRONMENT_TAG=$1
VPC_ID=$2
SUBNET_ID=$3
AUTH_EIP=$4
API_EIP=$5
FRONTEND_EIP=$6

aws cloudformation create-stack --stack-name ${ENVIRONMENT_TAG}-srm-all --template-body file://srm-all.json --parameters \
    ParameterKey=environment,ParameterValue=${ENVIRONMENT_TAG} \
    ParameterKey=vpcId,ParameterValue=${VPC_ID} \
    ParameterKey=subnet,ParameterValue=${SUBNET_ID} \
    ParameterKey=authEIP,ParameterValue=${AUTH_EIP} \
    ParameterKey=apiEIP,ParameterValue=${API_EIP} \
    ParameterKey=frontendEIP,ParameterValue=${FRONTEND_EIP}
