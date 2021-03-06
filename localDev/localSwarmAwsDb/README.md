# Running Application in local docker swarm (with aws databases)

The scripts and configuration in this folder will allow you to bring up the full BristechSRM system as a local docker swarm, and access frontend from windows on `http://localhost:8080`.
The Api gateway will also be avaiable on `localhost:8081` and the auth service on `localhost:9003`.

## Install Requirements 
You will require git bash or an equivalent installed, as well as Docker. On windows 7, we have used docker toolbox : https://www.docker.com/products/docker-toolbox. 
Tested on docker version 1.12.0.

## Setup 

Before running, all secrets config and general config must be created / modified. All config files are contained in ./configs/{application} The following lists what is required (Alphabetical order).

### Auth
 - The secrets config file for Auth must be placed under ./configs/auth/ as secrets.Auth.config. 
    - Note, the certificateFile setting value must be of the format "/certFileName.pfx" (There is an issue around the location of the file, so it will be place in the file system root for the service)
 - The corresponding certificate file must be placed under ./configs/auth/ . E.g. if the certificateFile specfied is "/certFileName.pfx", the file needs to be "certFileName.pfx".
   A template is ready at ./configs/auth/template.secrets.Auth.config

### Publish
- The secrets config file for Publish must be placed under ./configs/publish as Publish.exe.secrets. The meetup Api Key must be set. 
  A template is ready at ./configs/publish/template.Publish.exe.secrets.

## Running
To run the application, run `./setupAll.sh` and follow all instructions as prompted. You will also be required to run `./setupData.sh` after verifying that all services are running

## Shutdown and cleanup

Remove all VMs, run `./destroy.sh`
