#!/bin/sh -eu

apt-get -y install software-properties-common
apt-add-repository -y ppa:ansible/ansible
apt-get update
apt-get -y install ansible

cat << EOF >> /etc/hosts

10.0.15.11 box1
10.0.15.12 box2
10.0.15.13 box3

EOF
