#!/bin/sh -eu

PRIVATE_IP=$(ifconfig eth0 | awk '/inet addr/{split($2,a,":"); print a[2]}')
DOCKER_HOST="tcp://${PRIVATE_IP}:2375"

sudo docker run -H ${DOCKER_HOST} -d -p 8080:8080 -v /home/ubuntu/prd.comms.config:/service/Comms.exe.config bristechsrm/comms
