#!/bin/bash -euv
docker service create \
 --replicas 1 \
 --network srm-network \
 --name trello-migrate \
 --mount type=bind,source=/service/configs/trelloMigrate/trelloMigrate.app.config,target=/service/TrelloMigrate.exe.config \
 --constraint 'node.hostname == manager0' \
 --restart-condition none \
 trello-migrate
 