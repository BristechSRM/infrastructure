#!/bin/sh

buffer=$(cat)

serviceName=$(echo $buffer | jq '.source.serviceName')
nodeName=$(echo $buffer | jq '.source.nodeNode')
swarmMasterIp=$(echo $buffer | jq '.source.swarmMasterIp')
repository=$(echo $buffer | jq '.source.repository')
overlay=$(echo $buffer | jq '.source.overlay')

export DOCKER_HOST="tcp://${swarmMasterIp}:2376"

docker run -d -p 9001:9001 --name="${serviceName}" --net=${overlay} --env="constraint:node==${nodeName} ${repository}

