#!/bin/bash -euv
UUID=$(uuidgen)

echo "Email (to be used with google OAUTH Login. E.g. Scottlogic email):"
read EMAIL

echo "First name:"
read FIRST_NAME

echo "Last name:"
read LAST_NAME

docker service create --name addAuthUser \
--env AWS_ACCESS_KEY_ID=FAKE \
--env AWS_SECRET_ACCESS_KEY=FAKE \
--env AWS_DEFAULT_REGION=eu-west-1 \
--network srm-network \
 --constraint 'node.hostname == manager0' \
 --restart-condition none \
garland/aws-cli-docker aws dynamodb put-item --endpoint-url http://srm-dynamo:7000 \
--table-name Auth.Users \
--item \
"{\"Id\":{\"S\":\"$UUID\"},\"Email\":{\"S\":\"$EMAIL\"},\"Firstname\":{\"S\":\"$FIRST_NAME\"},\"Surname\":{\"S\":\"$LAST_NAME\"}}"
