
# ---------------
# Create Stack
# ---------------
./createStack.sh SRM vpc-d8c5debd subnet-488a9f11 52.51.54.255 52.50.88.117 52.50.27.136




# ---------------
# SSH onto Consul host and...
# ---------------
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/installDocker.sh | sh
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/runConsul.sh | sh

> curl http://localhost:8500/v1/kv

# ---------------
# SSH onto SwarmMaster and...
# ---------------
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/installDockerCluster.sh | sh -s --xxx--
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/runSwarmMaster.sh | sh -s --xxx--

> docker -H tcp://localhost:3376 info

# ---------------
# SSH onto all nodes and...
# ---------------
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/installDockerCluster.sh | sh -s --xxx--
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/runSwarmAgent.sh | sh -s --xxx--

# ---------------
# On any node...
# ---------------
docker network create --driver overlay overlay

> docker network ls





# ---------------
# SSH onto Comms host and...
# ---------------
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/prd.comms.config
# ADD SECRETS FILE!
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/runComms.sh | sh

> curl http://localhost:8080/last-contact

# ---------------
# SSH onto Sessions host and...
# ---------------
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/runSessions.sh | sh

> curl http://localhost:8080/sessions

# ---------------
# SSH onto Auth host and...
# ---------------
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/prd.auth.config
# ADD SECRETS FILE!
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/runAuth.sh | sh

> curl http://localhost:8080/.well-known/openid-configuration

# ---------------
# SSH onto ApiGateway host and...
# ---------------
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/prd.api.config
# CHECK URLs for nodes
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/runApiGateway.sh | sh

> curl http://localhost:8080/sessions

# ---------------
# SSH onto Frontend host and...
# ---------------
wget --quiet --cache off https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/prd.frontend.json
wget --quiet --cache off -O - https://raw.githubusercontent.com/BristechSRM/infrastructure/cloudFormation/cloudFormation/runFrontend.sh | sh

> docker ps
