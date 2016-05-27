# Usage
## Running locally
- Make sure the aws cli works
- Run ./up.sh
- Run ./utils/addAuthUser.sh
- When finished run ./down.sh

## Developing a service
- Run docker ps
- Run docker stop {SERVICE_NAME}
- Run ./utils/runProxy.sh
- Run service with correctly configured ip addresses and port mappings
