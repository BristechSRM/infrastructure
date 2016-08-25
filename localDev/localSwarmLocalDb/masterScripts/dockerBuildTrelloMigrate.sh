#!/bin/bash -euv
sudo rm -rf ~/source
sudo rm -rf ~/binaries
mkdir -p ~/source
mkdir -p ~/binaries
git clone --depth 1 https://github.com/BristechSRM/TrelloMigrate.git ~/source
docker run -v ~/source:/source \
    -v ~/binaries:/binaries \
    -v /service/configs/trelloMigrate/TrelloCreds.config:/config/TrelloCreds.config \
    -v ~/trelloMigrateBuild.sh:/trelloMigrateBuild.sh \
    bristechsrm/build-fsharp /trelloMigrateBuild.sh

cp /service/configs/trelloMigrate/Dockerfile ~/context/Dockerfile
cp -R ~/binaries/ ~/context/
cd ~/context/
docker build -t trello-migrate .
