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
  - (The services that are currently avaliable for swapping can be found in utils/servicePortMapping.sh
- Run ./utils/runProxy.sh
- Run service with correctly configured ip addresses and port mappings

## Stop developing a service (Return to original state) without reset
- Run docker ps -a (The proxy service will be called SERVICENAME_PROXY)
- Run docker stop {PROXY_SERVICE_NAME}
- Run docker start {ORIGINAL_SERVICE_NAME}
