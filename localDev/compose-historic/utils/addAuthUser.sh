#!/bin/bash
UUID=$(uuidgen)

echo "Email:"
read EMAIL

echo "First name:"
read FIRST_NAME

echo "Last name:"
read LAST_NAME

aws dynamodb put-item --endpoint-url http://localhost:7000 \
  --table-name Auth.Users \
  --item \
      "{\"Id\":{\"S\":\"$UUID\"},\"Email\":{\"S\":\"$EMAIL\"},\"Firstname\":{\"S\":\"$FIRST_NAME\"},\"Surname\":{\"S\":\"$LAST_NAME\"}}"
