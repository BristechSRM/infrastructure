#!/bin/bash
echo "Enter docker host ip: (eth# for example)"
read DOCKER_HOST_IP

./utils/buildConfigs.sh $DOCKER_HOST_IP
docker-compose up -d
