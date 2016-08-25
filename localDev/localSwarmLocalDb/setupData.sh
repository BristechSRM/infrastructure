#!/bin/bash -euv

echo "Setting up data and user login"
docker-machine ssh manager0 "~/addAuthUser.sh"
docker-machine ssh manager0 "~/dockerBuildTrelloMigrate.sh && ~/startTrelloMigrate.sh"

