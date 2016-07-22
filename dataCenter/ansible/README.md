Ansible
========================

Ansible only works on Unix, so we'll have a management VM and our nodes.

We'll be using -i PATH to select the environment root as suggested here:
http://toja.io/using-host-and-group-vars-files-in-ansible/



Local development
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
$ ansible-playbook -i env_local all-ssh-addkey.yml --ask-pass
password: isanopensecret
```

AWS
========================

You'll need a unix host to drive this.  "mgmt" will do.

Create the AWS stack under cloudFormation
Make sure the private keys for ssh to the AWS Instances are available
Update the addresses in the env_TAG variables and inventory



Go!
========================

Place the secrets files in /home/vagrant
------------------------
Auth.secrets.config
Comms.secrets.config


Check connectivity (and get any "add known_hosts" prompts over with)
------------------------
```
$ ansible -i env_TAG all -m ping
```


Docker and docker-py are universal
------------------------
$ ansible-playbook -i env_TAG all-apt-docker.yml
$ ansible-playbook -i env_TAG all-pip-docker-py.yml

Consul and Master
------------------------
$ ansible-playbook -i env_TAG infra-docker.yml

$ ansible-playbook -i env_TAG srm-consul.yml
$ ansible-playbook -i env_TAG srm-master.yml

Unleash Microservices
------------------------
$ ansible-playbook -i env_TAG nodes-docker-swarm.yml

$ ansible-playbook -i env_TAG srm-auth.yml
$ ansible-playbook -i env_TAG srm-comms.yml
$ ansible-playbook -i env_TAG srm-sessions.yml
$ ansible-playbook -i env_TAG srm-gateway.yml
$ ansible-playbook -i env_TAG srm-frontend.yml


Try the website.


Smoke tests
-----------------------
```
> vagrant ssh consul
$ curl http://localhost:8500/v1/kv
```

```
> vagrant ssh master
$ docker info
```

```
> vagrant ssh auth
$ curl http://localhost:8080/
```

```
> vagrant ssh comms
$ curl http://localhost:8080/last-contact
```

```
> vagrant ssh sessions
$ curl http://localhost:8080/sessions
```

```
> vagrant ssh gateway
$ curl http://localhost:8080/sessions
```

```
> vagrant ssh frontend
$ curl http://localhost:8080/
```
