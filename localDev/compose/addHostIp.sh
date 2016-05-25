#!/bin/bash
DOCKER_HOST_IP=$(ifconfig eth0 | grep 'inet addr:' | cut -d: -f2 | awk '{ print $1}')

sed -i.bak "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" addAuthUser.sh
sed -i.bak "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" api-gateway.app.config
sed -i.bak "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" auth.app.config
sed -i.bak "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" docker-compose.yml
sed -i.bak "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" frontend.config.json
sed -i.bak "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" sessions.app.config
sed -i.bak "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" addAuthUser.sh
