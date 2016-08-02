#!/bin/sh -eu

if [ "$#" -ne 7 ]; then
    echo "Usage: $0 environment-tag dbInstanceName username password vpcId dbSubnetGroupName servicesSubnetCIDR"
    exit 1
fi

ENVIRONMENT_TAG=$1
INSTANCE_NAME=$2
USERNAME=$3
PASSWORD=$4
VPC_ID=$5
DB_SUBNET_GROUP_NAME=$6
SERVICES_SUBNET_CIDR=$7

aws cloudformation create-stack --stack-name ${ENVIRONMENT_TAG}-rds --template-body file://rds-tag.json --parameters \
    ParameterKey=environment,ParameterValue=${ENVIRONMENT_TAG} \
    ParameterKey=instanceName,ParameterValue=${INSTANCE_NAME} \
    ParameterKey=masterUsername,ParameterValue=${USERNAME} \
    ParameterKey=masterPassword,ParameterValue=${PASSWORD} \
    ParameterKey=vpcId,ParameterValue=${VPC_ID} \
    ParameterKey=dbSubnetGroupName,ParameterValue=${DB_SUBNET_GROUP_NAME} \
    ParameterKey=servicesSubnetCIDR,ParameterValue=${SERVICES_SUBNET_CIDR}
