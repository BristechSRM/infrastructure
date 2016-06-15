#!/bin/sh -eu

aws cloudformation create-stack --stack-name pna-sg-all --template-body file://sg-all.json --parameters ParameterKey=vpcId,ParameterValue=vpc-d8c5debd
