
Ansible only works on Unix, so we'll have a management VM and our nodes.


Create the local VMs
------------------------
```
> vagrant up
```


Ansible mgmt node
------------------------
Log in using ssh
```
> vagrant ssh mgmt
$ ansible --version
```


Install ssh key
------------------------
Set us up for password-free use (i.e. generate a ssh key and install it on the nodes)
```
$ ssh-keygen -t rsa -b 2048
$ ssh-keyscan auth comms sessions gateway frontend >> ~/.ssh/known_hosts

$ cd /vagrant
$ ansible-playbook nodes-ssh-addkey.yml --ask-pass
password: isanopensecret

$ ansible all -m ping
```


Set up docker (all nodes the same)
-----------------------
```
$ ansible-playbook nodes-apt-docker.yml

> vagrant ssh auth
$ docker ps
```

```
$ ansible-playbook nodes-srm-docker.yml

> vagrant ssh comms
$ cat /etc/default/docker
```

```
$ ansible-playbook nodes-pip-docker-py.yml

> vagrant ssh sessions
$ pip list | grep docker-py
```


Install the individual images
-----------------------
--step used so we can install the secrets manually before the 'start'
```
$ ansible-playbook srm-auth.yml --step

> vagrant ssh auth
$ docker ps
```

```
$ ansible-playbook srm-comms.yml --step

> vagrant ssh comms
$ curl http://localhost:8080/last-contact
```

```
$ ansible-playbook srm-sessions.yml

> vagrant ssh sessions
$ curl http://localhost:8080/sessions
```

