#!/bin/sh -euv

#Create machines
docker-machine create --driver virtualbox manager0
docker-machine create --driver virtualbox worker0
docker-machine create --driver virtualbox worker1

#Setup config folders
docker-machine ssh manager0 "sudo mkdir /service/ && sudo chown docker /service/"
docker-machine ssh worker0 "sudo mkdir /service/ && sudo chown docker /service/"
docker-machine ssh worker1 "sudo mkdir /service/ && sudo chown docker /service/"

docker-machine scp -r configs/ manager0:/service/
docker-machine scp -r configs/ worker0:/service/
docker-machine scp -r configs/ worker1:/service/

#Setup the master as swarm master and get the masterIp and Join token
masterIp=$(docker-machine ip manager0)

eval $(docker-machine env --shell bash manager0)

docker swarm init --advertise-addr $masterIp
joinToken=$(docker swarm join-token -q worker)

#Add worker0 to swarm
eval $(docker-machine env --shell bash worker0)
docker swarm join --token $joinToken "$masterIp:2377"

#Add worker01 to swarm
eval $(docker-machine env --shell bash worker1)
docker swarm join --token $joinToken "$masterIp:2377"

eval $(docker-machine env --shell bash manager0)

docker network create --driver overlay srm-network

"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe" controlvm "manager0" natpf1 "frontend,tcp,127.0.0.1,8080,,8080"
"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe" controlvm "manager0" natpf1 "gateway,tcp,127.0.0.1,8081,,8081"
"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe" controlvm "manager0" natpf1 "auth,tcp,127.0.0.1,9003,,9003"

docker-machine scp setupServices.sh manager0:~/setupServices.sh

echo "SSHing into manager0 and bringing up services"
docker-machine ssh manager0 "sleep 30; ./setupServices.sh"

eval $(docker-machine env --shell bash manager0)
