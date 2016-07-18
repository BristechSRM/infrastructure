
Manual Build instructions
=================


Consul host
-----------------
ssh in, needs docker (no cluser) and consul
```
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/installDocker.sh | sh
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runConsul.sh | sh

$ curl http://localhost:8500/v1/kv
```


SwarmMaster
-----------------
ssh in, needs docker cluser and Swarm Master
```
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/installDockerCluster.sh | sh -s --xxx--
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runSwarmMaster.sh | sh -s --xxx--

$ docker -H tcp://localhost:3376 info
```


All nodes
-----------------
ssh in, need docker cluser and Swarm Agent
```
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/installDockerCluster.sh | sh -s --xxx--
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runSwarmAgent.sh | sh -s --xxx--
```


Overlay network
-----------------
Not used at present, but f.y.i.
```
docker network create --driver overlay overlay

$ docker network ls
```



Auth
-----------------
Add secrets file
```
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/prd.auth.config
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runAuth.sh | sh

$ curl http://localhost:8080/.well-known/openid-configuration
```


Comms
-----------------
Add secrets file
```
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/prd.comms.config
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runComms.sh | sh

$ curl http://localhost:8080/last-contact
```


Sessions
-----------------
```
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runSessions.sh | sh

$ curl http://localhost:8080/sessions
```


ApiGateway
-----------------
Set config file URLs
```
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/prd.api.config
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runApiGateway.sh | sh

$ curl http://localhost:8080/sessions
```


Frontend
-----------------
```
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/prd.frontend.json
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runFrontend.sh | sh

$ docker ps
```


Startup
-----------------
Point your DOCKER_HOST at the Swarm Master and
```
docker run -d -p 8080:8080 --name=comms --net=bridge --env=constraint:node==ip-10-0-1-116 \
    -v /home/ubuntu/prd.comms.config:/service/Comms.exe.config -v /home/ubuntu/secrets.comms.config:/service/secrets.config bristechsrm/comms

docker run -d -p 8080:8080 --name=sessions --net=bridge --env=constraint:node==ip-10-0-1-156 bristechsrm/sessions

docker run -d -p 8080:8080 --name=auth --net=bridge --env=constraint:node==ip-10-0-1-218 \
    -v /home/ubuntu/prd.auth.config:/service/Auth.exe.config -v /home/ubuntu/secrets.auth.config:/service/secrets.config bristechsrm/auth

docker run -d -p 8080:8080 --name=apigateway --net=bridge --env=constraint:node==ip-10-0-1-32 \
    -v /home/ubuntu/prd.apigateway.config:/service/ApiGateway.exe.config bristechsrm/api-gateway

docker run -d -p 80:8080 --name=frontend --net=bridge --env=constraint:node==ip-10-0-1-242 bristechsrm/frontend
```
