#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
BUILD_DIR="$DIR/../build"

echo "Enter service id:"
read SERVICE_ID
SERVICE_PORT=$($DIR/servicePortMapping.sh $SERVICE_ID)

echo "Enter host ip:"
read HOST_IP

sed "s/{PROXY_IP}/$HOST_IP/;s/{PORT}/$SERVICE_PORT/" $DIR/../config/proxy.default.conf > $BUILD_DIR/$SERVICE_ID.default.conf

docker run -d --name ${SERVICE_ID}_proxy -v $BUILD_DIR/$SERVICE_ID.default.conf:/etc/nginx/conf.d/default.conf -p $SERVICE_PORT:$SERVICE_PORT nginx
