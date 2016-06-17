#!/bin/sh -eu

if [ "$#" -ne 1 ]; then
    echo "Usage: $0 consul-ip"
    exit 1
fi

CONSUL_IP=$1
PRIVATE_IP=$(ifconfig eth0 | awk '/inet addr/{split($2,a,":"); print a[2]}')
DOCKER_HOST="tcp://${PRIVATE_IP}:2375"

sudo docker -H ${DOCKER_HOST} run -d swarm join --advertise ${PRIVATE_IP}:2375 consul://${CONSUL_IP}:8500
