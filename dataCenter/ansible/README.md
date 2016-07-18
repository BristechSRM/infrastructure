
Ansible only works on Unix, so we'll have a management VM and our nodes.
We'll be using -i PATH to select the environment root.


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
$ ssh-keygen -t rsa -b 2048
$ ssh-keyscan auth comms sessions gateway frontend consul >> ~/.ssh/known_hosts

$ cd /vagrant
$ ansible-playbook -i env_local all-ssh-addkey.yml --ask-pass
password: isanopensecret

$ ansible -i env_local all -m ping
```


Place the secrets files in /home/vagrant
------------------------
Auth.secrets.config
Comms.secrets.config


Unleash DEV
------------------------
$ ansible-playbook -i env_local srm-all.yml


Smoke tests
-----------------------
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

Try the website.

```
> vagrant ssh consul
$ curl http://localhost:8500/v1/kv
```
