
Cloud Formation instructions
=================

Create Stack
-----------------
Use a Unix box with aws cli,

or make one...
```
> vagrant up
> vagrant ssh driver

$ aws configure
```

Run the wrapper script with appropriate TAG
```
$ cd /vagrant
./srm-tag.sh TAG vpc-d8c5debd subnet-488a9f11 10.0.5.0/24
```

For PRD, set up the Elastic IPs as follows:
Auth: 52.51.54.255
Gateway: 52.50.88.117
Frontend: 52.50.27.136
