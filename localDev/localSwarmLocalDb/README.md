# Running Application in local docker swarm (with local databases)

The scripts and configuration in this folder will allow you to bring up the full BristechSRM system as a local docker swarm, and access frontend from windows on `http://localhost:8080`.
The Api gateway will also be avaiable on `localhost:8081` and the auth service on `localhost:9003`.

## Install Requirements 
You will require git bash or an equivalent installed, as well as Docker. On windows 7, we have used docker toolbox : https://www.docker.com/products/docker-toolbox. 
Note for git bash, you either need to use the executable "C:\Program Files\Git\git-bash.exe", or call bash or sh (found in "C:\Program Files\Git\bin\") with the -l (--login) flag. 
This makes sure that the shell environment is setup properly
Tested on docker version 1.12.0.

## Setup 

Before running, all secrets config and general config must be created / modified. All config files are contained in ./configs/{application} The following lists what is required (Alphabetical order).

### Auth
 - The secrets config file for Auth must be placed under ./configs/auth/ as secrets.Auth.config. 
    - Note, the certificateFile setting value must be of the format "/certFileName.pfx" (There is an issue around the location of the file, so it will be place in the file system root for the service)
    - Note, for the settings for the AWSAccessKey and AWSSecretKey: Although credentials will not be used for the local dynamoDb, a value is still required. 
  Either enter valid values or enter "FAKE" in the value for the AWSAccessKey and AWSSecretKey.
 - The corresponding certificate file must be placed under ./configs/auth/ . E.g. if the certificateFile specfied is "/certFileName.pfx", the file needs to be "certFileName.pfx".
   A template is ready at ./configs/comms/template.secrets.Auth.config

### Comms
 - The secrets config file for Comms must be placed under ./configs/comms/ as secrets.Comms.config. Although credentials will not be used for the local dynamoDb, a value is still required. 
  Either enter valid values or enter "FAKE" in the value for the AWSAccessKey and AWSSecretKey.
  A template is ready at ./configs/comms/template.secrets.Comms.config

### TrelloMigrate 
 - The trello credentials config file for TrelloMigrate must be placed under ./configs/trelloMigrate as TrelloCreds.config. 
 To create this, see the readMe of the TrelloMigrate project : <https://github.com/BristechSRM/TrelloMigrate>.
 A template is ready at ./configs/trelloMigrate/template.TrelloCreds.config

## Running
To run the application, run `./setupAll.sh` and follow all instructions as prompted. You will also be required to run `./setupData.sh` after verifying that all services are running

## Shutdown and cleanup

Remove all VMs, run `./destroy.sh`