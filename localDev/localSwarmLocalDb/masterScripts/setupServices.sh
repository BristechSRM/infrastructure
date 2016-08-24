#!/bin/sh -euv

docker service create \
    --replicas 1 \
    --network srm-network \
    --env MYSQL_ROOT_PASSWORD=apassword \
    --env MYSQL_USER=sessions \
    --env MYSQL_PASSWORD=apassword \
    --name sessions-mysql \
    bristechsrm/srm-mysql-db

docker service create \
    --replicas 1 \
    --network srm-network \
    --name srm-dynamo \
    bristechsrm/srm-dynamo-db

docker service create \
    --replicas 1 \
    --network srm-network \
    --mount type=bind,source=/service/configs/sessions/sessions.app.config,target=/service/Sessions.exe.config \
    --name sessions \
    bristechsrm/sessions

docker service create \
    --replicas 1 \
    --network srm-network \
    --mount type=bind,source=/service/configs/comms/secrets.Comms.config,target=/service/secrets.config \
    --mount type=bind,source=/service/configs/comms/comms.app.config,target=/service/Comms.exe.config \
    --name comms \
    bristechsrm/comms

docker service create --publish 9003:8080 \
        --replicas 1 \
        --network srm-network \
        --mount type=bind,source=/service/configs/auth/auth.app.config,target=/service/Auth.exe.config \
        --mount type=bind,source=/service/configs/auth/secrets.Auth.config,target=/service/secrets.config \
        --mount type=bind,source=/service/configs/auth/AuthCertificate.pfx,target=/AuthCertificate.pfx \
        --name auth \
        bristechsrm/auth

docker service create --publish 8081:8080 \
    --replicas 1 \
    --network srm-network \
    --mount type=bind,source=/service/configs/gateway/gateway.app.config,target=/service/Gateway.exe.config \
    --name gateway \
    bristechsrm/gateway

docker service create --publish 8080:8080 \
    --replicas 1 \
    --network srm-network \
    --mount type=bind,source=/service/configs/frontend/frontend.json,target=/frontend/public/js/config.json \
    --name frontend \
    bristechsrm/frontend
