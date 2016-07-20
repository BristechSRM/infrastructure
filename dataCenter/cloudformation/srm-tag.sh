#!/bin/sh -eu

if [ "$#" -ne 4 ]; then
    echo "Usage: $0 environment-tag vpc-id subnet-id concourse-subnet-cidr"
    exit 1
fi

ENVIRONMENT_TAG=$1
VPC_ID=$2
SUBNET_ID=$3
CONCOURSE_SUBNET_CIDR=$4

aws cloudformation create-stack --stack-name ${ENVIRONMENT_TAG}-srm --template-body file://srm-tag.json --parameters \
    ParameterKey=environment,ParameterValue=${ENVIRONMENT_TAG} \
    ParameterKey=vpcId,ParameterValue=${VPC_ID} \
    ParameterKey=servicesSubnetId,ParameterValue=${SUBNET_ID} \
    ParameterKey=concourseSubnetCIDR,ParameterValue=${CONCOURSE_SUBNET_CIDR}
