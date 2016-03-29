# Concourse resource to update an ECS container

## Cloud Formation Cluster

Can be used with a cloud formation stack with the following configuration
```
- name: cluster
  type: ecs
    source:
      stackName: 
      clusterName:
      taskName:
      serviceName:
      accessKeyId: {{awsAccessKeyId}}
      secretAccessKey: {{awsSecretAccessKey}}

```

Where those there names are the names in the cloud formation template

## Manual Stack

Can be used with a manually set up cluster with this configuration:
```
- name: cluster
  type: ecs
    source:
      clusterName:
      taskName:
      serviceName:
      accessKeyId: {{awsAccessKeyId}}
      secretAccessKey: {{awsSecretAccessKey}}
```
Where those there names are the actual resource names of the those things
