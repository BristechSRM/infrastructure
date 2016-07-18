#!/bin/sh -eu

apt-get -y install software-properties-common
apt-add-repository -y ppa:ansible/ansible
apt-get update
apt-get -y install ansible

cat << EOF >> /etc/hosts

10.0.15.11 auth
10.0.15.12 comms
10.0.15.13 sessions
10.0.15.14 gateway
10.0.15.15 frontend

10.0.15.20 consul
EOF
