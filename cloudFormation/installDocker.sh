#!/bin/sh -eu

PRIVATE_IP=$(ifconfig eth0 | awk '/inet addr/{split($2,a,":"); print a[2]}')

sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 36A1D7869245C8950F966E92D8576A8BA88D21E9
sudo sh -c "echo 'deb https://get.docker.io/ubuntu docker main' >> /etc/apt/sources.list"
sudo apt-get update
sudo apt-get -y install lxc-docker-1.9.1=1.9.1
sudo sh -c "echo 'DOCKER_OPTS=\"-D -H tcp://'${PRIVATE_IP}':2375\"' >> /etc/default/docker"
sudo restart docker
sudo usermod -aG docker ubuntu

echo "You'll need to logout for the group change to take effect"
exit 0
