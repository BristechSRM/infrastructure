#!/bin/sh -eu

apt-get update
apt-get install -y python-pip
pip install --upgrade pip
pip install awscli
