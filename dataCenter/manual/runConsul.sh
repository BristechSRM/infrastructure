#!/bin/sh -eu

PRIVATE_IP=$(ifconfig eth0 | awk '/inet addr/{split($2,a,":"); print a[2]}')
DOCKER_HOST="tcp://${PRIVATE_IP}:2375"

sudo docker -H ${DOCKER_HOST} run -d -p "8500:8500" -h "consul" progrium/consul -server -bootstrap -log-level=trace
