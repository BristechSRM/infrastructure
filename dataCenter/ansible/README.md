Ansible
========================

Ansible only works on Unix, so we'll have a management VM and our nodes.

We'll be using -i PATH to select the environment root as suggested here:
http://toja.io/using-host-and-group-vars-files-in-ansible/



On a local swarm
========================

Create the local VMs
------------------------
```
> vagrant up
```


Set up a VM to run Ansible on - "mgmt"
------------------------
Log in using ssh and check Ansible is installed
```
> vagrant ssh mgmt
$ ansible --version
```

Set us up for password-free use (i.e. generate a ssh key and install it on the nodes)
```
$ ssh-keyscan box1 box2 box3 >> ~/.ssh/known_hosts

$ ssh-keygen -t rsa -b 2048
$ cd /vagrant
$ ansible-playbook -i env_local nodes-ssh-addkey.yml --ask-pass
password: <isanopensecret>
```

On AWS
========================

You'll need a unix host to drive this.  The "mgmt" VM will do.

Create the AWS stack under "cloudFormation".
Make sure the private keys for ssh to the AWS Instances are available on your host.
Update the locations and urls in the env_local variables and inventory.



Go!
========================

Place the appropriate secrets files in the folder with this README.
------------------------
```
AuthCertificate.pfx
Auth.exe.secrets
Comms.exe.secrets
```

Check connectivity (and get any "add known_hosts" prompts over with)
------------------------
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

Try the website.  Well, wait a bit and try the website.



Smoke tests
-----------------------

auth
```
$ curl http://localhost:9003/
```

gateway
```
$ curl http://localhost:9090/sessions
```

frontend
```
$ curl http://localhost:8080/
```

Diagnostics
-----------------------

On the swarm master:
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
