
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

Run the appropriate wrapper script
for PRD
```
$ cd /vagrant
./srm-all.sh SRM vpc-d8c5debd subnet-488a9f11 10.0.5.0/24 52.51.54.255 52.50.88.117 52.50.27.136
```
or development TAG
```
$ cd /vagrant
./srm-tag.sh TAG vpc-d8c5debd subnet-488a9f11 10.0.5.0/24
```
