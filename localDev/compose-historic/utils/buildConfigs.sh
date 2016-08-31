#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
BUILD_DIR="$DIR/../build"
CONFIG_DIR="$DIR/../config"

DOCKER_HOST_IP=$1

mkdir -p $BUILD_DIR
cp $CONFIG_DIR/frontend.config.json $BUILD_DIR/
sed "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" $CONFIG_DIR/api-gateway.app.config > $BUILD_DIR/api-gateway.app.config
sed "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" $CONFIG_DIR/comms.app.config > $BUILD_DIR/comms.app.config
sed "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" $CONFIG_DIR/auth.app.config > $BUILD_DIR/auth.app.config
sed "s/{DOCKER_HOST_IP}/$DOCKER_HOST_IP/" $CONFIG_DIR/sessions.app.config > $BUILD_DIR/sessions.app.config
