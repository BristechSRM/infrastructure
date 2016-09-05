
Docker Swarm Deploy
===================

Our Concourse pipelines use a custom resource (bristechsrm/docker-swarm-deploy) to deploy to Docker Swarm.
The resource is in the form of a Docker image on Docker Hub.
This Dockerfile builds that image.


Building
-------------------

You need a unix VM.
You need to borrow the team login to dockerhub.
Then docker login, build, and push the image.

```
$ cd /vagrant
$ docker login
Username:
Password:

$ docker build -t bristechsrm/docker-swarm-deploy .
$ docker push bristechsrm/docker-swarm-deploy
```
