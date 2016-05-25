#!/bin/bash
if [ "$3" == "" ]; then
    echo "ERROR: email, firstname & surname required as command line arguments"
    exit 1
fi

uuid=$(uuidgen)
email=$1
firstname=$2
surname=$3

aws dynamodb put-item --endpoint-url http://localhost:7000 \
  --table-name Auth.Users \
  --item \
      "{\"Id\":{\"S\":\"$uuid\"},\"Email\":{\"S\":\"$email\"},\"Firstname\":{\"S\":\"$firstname\"},\"Surname\":{\"S\":\"$surname\"}}"
