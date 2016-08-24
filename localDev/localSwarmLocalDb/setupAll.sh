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

eval $(docker-machine env manager0)

docker swarm init --advertise-addr $masterIp
joinToken=$(docker swarm join-token -q worker)

#Add worker0 to swarm
eval $(docker-machine env worker0)
docker swarm join --token $joinToken "$masterIp:2377"

#Add worker01 to swarm
eval $(docker-machine env worker1)
docker swarm join --token $joinToken "$masterIp:2377"

eval $(docker-machine env manager0)

docker network create --driver overlay srm-network

"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe" controlvm "manager0" natpf1 "frontend,tcp,127.0.0.1,8080,,8080"
"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe" controlvm "manager0" natpf1 "gateway,tcp,127.0.0.1,8081,,8081"
"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe" controlvm "manager0" natpf1 "auth,tcp,127.0.0.1,9003,,9003"

docker-machine scp -r masterScripts/ manager0:~/ && docker-machine ssh manager0 "cp -r ~/masterScripts/. ~/ && rm -rf ~/masterScripts"

echo "SSHing into manager0 and bringing up services"
docker-machine ssh manager0 "sleep 30; ~/setupServices.sh"

eval $(docker-machine env manager0)

echo "Wait until services are running. This may take several minutes."
echo "Run 'docker service ls' and ensure all service have the correct number of replicas, e.g. 1/1"
echo "For final confirmation before the next step, copy and run the command below. All services should be in a running state: "
echo "docker service ps sessions && docker service ps comms && docker service ps sessions-mysql"

echo "Final Step: Run ./setupData.sh from windows"


