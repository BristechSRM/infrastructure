#!/bin/sh -eu

if [ "$#" -ne 7 ]; then
    echo "Usage: $0 environment-tag vpc-id subnet-id concourse-subnet-cidr authEIP apiEIP frontendEIP"
    exit 1
fi

ENVIRONMENT_TAG=$1
VPC_ID=$2
SUBNET_ID=$3
CONCOURSE_SUBNET_CIDR=$4
AUTH_EIP=$5
API_EIP=$6
FRONTEND_EIP=$7

aws cloudformation create-stack --stack-name ${ENVIRONMENT_TAG}-srm-all --template-body file://srm-all.json --parameters \
    ParameterKey=environment,ParameterValue=${ENVIRONMENT_TAG} \
    ParameterKey=vpcId,ParameterValue=${VPC_ID} \
    ParameterKey=servicesSubnetId,ParameterValue=${SUBNET_ID} \
    ParameterKey=concourseSubnetCIDR,ParameterValue=${CONCOURSE_SUBNET_CIDR} \
    ParameterKey=authEIP,ParameterValue=${AUTH_EIP} \
    ParameterKey=apiEIP,ParameterValue=${API_EIP} \
    ParameterKey=frontendEIP,ParameterValue=${FRONTEND_EIP}
