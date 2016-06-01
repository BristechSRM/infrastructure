# Usage
## Running locally
- Make sure the aws cli works (Make sure your credentials have been stored)
- Run ifconfig to find the ip required in the up script. If you are using our vagrant setup, this should be the same as the private_network ip specified in the Vagrantfile, but defer to what is available in ifconfig
- Run ./up.sh
- Run ./utils/addAuthUser.sh
- When finished run ./down.sh

## Developing a service
- Run docker ps
- Run docker stop {SERVICE_NAME}
- Run ./utils/runProxy.sh
- Run service with correctly configured ip addresses and port mappings
