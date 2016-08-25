Ansible
========================

Ansible only works on Unix, so we'll have a management VM and our nodes.

We'll be using -i PATH to select the environment root as suggested here:
http://toja.io/using-host-and-group-vars-files-in-ansible/



On multiple local VMs
========================

Create the local VMs
------------------------
```
> vagrant up
```


Set up Ansible mgmt node
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
$ ansible-playbook -i env_PNA nodes-ssh-addkey.yml --ask-pass
password: <isanopensecret>
```

On AWS
========================

You'll need a unix host to drive this.  "mgmt" will do.

Create the AWS stack under "cloudFormation".
Make sure the private keys for ssh to the AWS Instances are available on your host.
Update the locations and urls in the env_PNA variables and inventory.



Go!
========================

Place the secrets files in /home/vagrant (ubuntu on AWS)
------------------------
```
AuthCertificate.pfx
secrets.Auth.config
secrets.Comms.config
```

Check connectivity (and get any "add known_hosts" prompts over with)
------------------------
```
$ ansible -i env_PNA all -m ping
```

Install and configure docker
------------------------
```
$ ansible-playbook -i env_PNA nodes-apt-docker.yml
```


All config/secrets must go on all nodes
------------------------
```
$ ansible-playbook -i env_PNA nodes-srm-config.yml
```

Set up the swarm
------------------------
```
$ ansible-playbook -i env_PNA srm-master.yml
```
cut and paste the token and master address into variables, then
```
$ ansible-playbook -i env_PNA srm-agents.yml
$ ansible-playbook -i env_PNA srm-overlay.yml
```

Unleash Microservices
------------------------
```
$ ansible-playbook -i env_PNA srm-auth.yml
$ ansible-playbook -i env_PNA srm-comms.yml
$ ansible-playbook -i env_PNA srm-sessions.yml
$ ansible-playbook -i env_PNA srm-gateway.yml
$ ansible-playbook -i env_PNA srm-frontend.yml
```

Try the website.



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
