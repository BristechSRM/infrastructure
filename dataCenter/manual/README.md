
Manual Build instructions
=================


SSH onto Consul host and...
-----------------
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/installDocker.sh | sh
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runConsul.sh | sh

> curl http://localhost:8500/v1/kv

SSH onto SwarmdataCenter and...
-----------------
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/installDockerCluster.sh | sh -s --xxx--
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runSwarmdataCenter.sh | sh -s --xxx--

> docker -H tcp://localhost:3376 info

SSH onto all nodes and...
-----------------
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/installDockerCluster.sh | sh -s --xxx--
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runSwarmAgent.sh | sh -s --xxx--

On any node...
-----------------
docker network create --driver overlay overlay

> docker network ls




SSH onto Comms host and...
-----------------
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/prd.comms.config
# ADD SECRETS FILE!
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runComms.sh | sh

> curl http://localhost:8080/last-contact

SSH onto Sessions host and...
-----------------
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runSessions.sh | sh

> curl http://localhost:8080/sessions

SSH onto Auth host and...
-----------------
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/prd.auth.config
# ADD SECRETS FILE!
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runAuth.sh | sh

> curl http://localhost:8080/.well-known/openid-configuration

SSH onto ApiGateway host and...
-----------------
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/prd.api.config
# CHECK URLs for nodes
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runApiGateway.sh | sh

> curl http://localhost:8080/sessions

SSH onto Frontend host and...
-----------------
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/prd.frontend.json
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/master/dataCenter/manual/runFrontend.sh | sh

> docker ps



Swarm Master startup
-----------------
docker run -d -p 8080:8080 --name=comms --net=bridge --env=constraint:node==ip-10-0-1-116 \
    -v /home/ubuntu/prd.comms.config:/service/Comms.exe.config -v /home/ubuntu/secrets.comms.config:/service/secrets.config bristechsrm/comms

docker run -d -p 8080:8080 --name=sessions --net=bridge --env=constraint:node==ip-10-0-1-156 bristechsrm/sessions

docker run -d -p 8080:8080 --name=auth --net=bridge --env=constraint:node==ip-10-0-1-218 \
    -v /home/ubuntu/prd.auth.config:/service/Auth.exe.config -v /home/ubuntu/secrets.auth.config:/service/secrets.config bristechsrm/auth

docker run -d -p 8080:8080 --name=apigateway --net=bridge --env=constraint:node==ip-10-0-1-32 \
    -v /home/ubuntu/prd.apigateway.config:/service/ApiGateway.exe.config bristechsrm/api-gateway

docker run -d -p 80:8080 --name=frontend --net=bridge --env=constraint:node==ip-10-0-1-242 bristechsrm/frontend
