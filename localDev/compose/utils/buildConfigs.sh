#!/bin/bash
DOCKER_HOST_IP=$(ifconfig eth0 | grep 'inet addr:' | cut -d: -f2 | awk '{ print $1}')
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

mkdir -p $DIR/../build
cp $DIR/../config/frontend.config.json $DIR/../build/
sed "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" $DIR/../config/api-gateway.app.config > $DIR/../build/api-gateway.app.config
sed "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" $DIR/../config/comms.app.config > $DIR/../build/comms.app.config
sed "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" $DIR/../config/auth.app.config > $DIR/../build/auth.app.config
sed "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" $DIR/../config/sessions.app.config > $DIR/../build/sessions.app.config
