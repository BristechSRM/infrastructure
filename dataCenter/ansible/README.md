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

For production on AWS
========================

You will need a unix host to drive this.  The "mgmt" VM will do.

Create the AWS stack under "cloudFormation".
Make sure the private keys for ssh to the AWS Instances are available on your host.
Update the locations and urls in the env_local variables and inventory.



Go!
========================

The setup relies on secrets files that must not be checked in.  Ask a team member where to get them.
You need to put them in the folder with this README, with these names:
```
AuthCertificate.pfx
Auth.exe.secrets
Comms.exe.secrets
```

Check you understand "-i env\_local" vs. "-i env\_SRM"

Check connectivity
------------------------
(and get any "add known_hosts" prompts over with)
```
$ ansible -i env_local all -m ping
```

Set up the docker swarm and the overlay
------------------------
```
$ ansible-playbook -i env_local srm-swarm.yml
```

Unleash Microservices
------------------------
```
$ ansible-playbook -i env_local srm-microservices.yml
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
