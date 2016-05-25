#!/bin/bash
export DOCKER_HOST_IP=$(ifconfig eth0 | grep 'inet addr:' | cut -d: -f2 | awk '{ print $1}')
export PWD=$(pwd)
docker-compose up -d
