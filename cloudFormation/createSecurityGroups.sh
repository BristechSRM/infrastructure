#!/bin/sh -eu

aws cloudformation create-stack --stack-name pna-sg-ssh-ScottLogic --template-body file://sg-ssh-ScottLogic.json
aws cloudformation create-stack --stack-name pna-sg-2375-subnet --template-body file://sg-2375-subnet.json
aws cloudformation create-stack --stack-name pna-sg-8500-subnet --template-body file://sg-8500-subnet.json
aws cloudformation create-stack --stack-name pna-sg-8080-subnet --template-body file://sg-8080-subnet.json
aws cloudformation create-stack --stack-name pna-sg-8080-ScottLogic --template-body file://sg-8080-ScottLogic.json
aws cloudformation create-stack --stack-name pna-sg-80-ScottLogic --template-body file://sg-80-ScottLogic.json
