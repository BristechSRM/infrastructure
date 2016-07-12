
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

