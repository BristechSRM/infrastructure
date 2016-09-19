Ansible
========================

Ansible only works on Unix, so we'll have a management VM and our nodes.

We will be using -i PATH to select the environment root directory as suggested here:
http://toja.io/using-host-and-group-vars-files-in-ansible/.  Valid values are
"env\_local" and "env\_SRM".


For a local development swarm
========================

Create the local VMs
------------------------
```
> vagrant up
```


Set up the VM to run Ansible on - "mgmt"
------------------------
Log in using ssh and check Ansible is installed
```
> vagrant ssh mgmt
$ ansible --version
$ cd /vagrant
```

We don't want to type a password each time, so we will set up ssh known hosts and authorized keys.
Get the host keys for the nodes:
```
$ ssh-keyscan box1 box2 box3 >> ~/.ssh/known_hosts
```
Create an RSA key for this host.  You can accept the defaults for the keygen questions.
```
$ ssh-keygen -t rsa -b 2048
```
And push it to all the nodes with Ansible using a password.  You will be prompted for vagrant's
password.  Ask someone what that is.  Anyone at all.
```
$ ansible-playbook -i env_local nodes-ssh-addkey.yml --ask-pass
```

Search and change ansible_eth0 to ansible_eth1 in the roles.  This is due to the VM's Host only network being eth0.
It should be possible to fix this with some python and a flag in the group_vars...



For production on AWS
========================

You will need a unix host to drive this.  The "mgmt" VM will do.

Create the AWS stack under "cloudFormation".
Make sure the private keys for ssh to the AWS Instances are available on your host. Copy them to the location specified in the inventory file specified by `ansible_ssh_private_key_file` (or update the location). 

NOTE: If using vagrant to create the mgmt box, you will need to use a different location than the default shared `/vagrant`, as you will not be able to set the permissions of the key file correctly. 
If necessary, limit the permissions on the key file with `chmod 400 nodes.pem`.

Update the locations and urls in the env_SRM variables and inventory.




Go!
========================

The setup relies on secrets files that must not be checked in.  Ask a team member where to get them.
You need to put them in the folder with this README, with these names:
```
AuthCertificate.pfx
Auth.exe.secrets
Comms.exe.secrets
Publish.exe.secrets
```

Check you understand "-i env_local" vs. "-i env_SRM"
Check that the docker roles use eth1 (local swarm) or eth0 (AWS).


Check connectivity
------------------------
(and get any "add known_hosts" prompts over with)
```
$ ansible -i env_SRM all -m ping
```

Set up the docker swarm and the overlay
------------------------
```
$ ansible-playbook -i env_SRM srm-swarm.yml
```

Unleash Microservices
------------------------
```
$ ansible-playbook -i env_SRM srm-microservices.yml
```

Try the website at http://localhost:8080/.  Well, wait a bit and try the website.

The site is password protected, so you will be redirected to a Google sign-on and back.
If you type your password correctly and still get a Login ERROR, ask an admin about being
granted access.



Diagnostics
-----------------------
If the website is not working, checking the individual services with curl may narrow down the cause.
Just a response vs. no response is your first clue.  You can do these on any of the nodes.

auth service
```
$ curl http://localhost:9003/
```

gateway service
```
$ curl http://localhost:9090/sessions
```

frontend service
```
$ curl http://localhost:8080/
```

-----------------------
On the swarm master (box3):
```
$ docker network ls

$ docker service ls
$ docker service ps <serviceName>
```

On a node:
```
$ docker network inspect <networkName>

$ docker ps
$ docker logs <containerId>

$ docker exec -it <containerId> /bin/bash
$ ping <microserviceName>
```


Bringing it all down
========================
```
> vagrant destroy -f
```
