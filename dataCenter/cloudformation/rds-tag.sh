#!/bin/sh -eu

if [ "$#" -ne 2 ]; then
    echo "Usage: $0 environment-tag"
    exit 1
fi

ENVIRONMENT_TAG=$1
VPC_ID=$2

aws cloudformation create-stack --stack-name ${ENVIRONMENT_TAG}-rds --template-body file://rds-tag.json --parameters \
    ParameterKey=environment,ParameterValue=${ENVIRONMENT_TAG} \
    ParameterKey=vpcId,ParameterValue=${VPC_ID}
