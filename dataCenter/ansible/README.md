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
$ ssh-keyscan auth comms sessions gateway frontend consul master >> ~/.ssh/known_hosts

$ ssh-keygen -t rsa -b 2048
$ cd /vagrant
$ ansible-playbook -i env_SRM all-ssh-addkey.yml --ask-pass
password: isanopensecret
```

On AWS
========================

You'll need a unix host to drive this.  "mgmt" will do.

Create the AWS stack under "cloudFormation"
Make sure the private keys for ssh to the AWS Instances are available on your host
Update the locations and urls in the env_SRM variables and inventory



Go!
========================

Place the secrets files in /home/vagrant (ubuntu on AWS)
------------------------
AuthCertificate.pfx
secrets.Auth.config
secrets.Comms.config


Check connectivity (and get any "add known_hosts" prompts over with)
------------------------
```
$ ansible -i env_SRM all -m ping
```


Docker and docker-py are universal
------------------------
$ ansible-playbook -i env_SRM all-apt-docker.yml
$ ansible-playbook -i env_SRM all-pip-docker-py.yml

Infra just gets docker, nodes are a docker cluster
------------------------
$ ansible-playbook -i env_SRM infra-docker.yml
$ ansible-playbook -i env_SRM nodes-docker-swarm.yml

Consul and Master
------------------------
$ ansible-playbook -i env_SRM srm-do-infra.yml

Unleash Microservices
------------------------
$ ansible-playbook -i env_SRM srm-do-nodes.yml

Try the website.


Smoke tests
-----------------------
consul
```
$ curl http://localhost:8500/v1/kv
```

master
```
$ docker info
```

auth
```
$ curl http://localhost:8080/
```

comms
```
$ curl http://localhost:8080/last-contact
```

sessions
```
$ curl http://localhost:8080/sessions
```

gateway
```
$ curl http://localhost:8080/sessions
```

frontend
```
$ curl http://localhost:8080/
```
