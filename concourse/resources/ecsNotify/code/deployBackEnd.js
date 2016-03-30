var fs = require('fs');
var Promise = require("promise");
var AWS = require('aws-sdk');
AWS.config.update({region: 'eu-west-1'});
var ECS;
var CloudFormation;

var taskName;
var stackName;
var serviceName;
var clusterName;

var clusterArn;
var serviceArn;
var taskArn;

var stdin = process.stdin, inputChunks = [];
stdin.resume();
stdin.setEncoding('utf8');
stdin.on('data', function(chunk) {
    inputChunks.push(chunk);
});
stdin.on('end', readConfig);

function readConfig() {
    var parsedData = JSON.parse(inputChunks.join());

    AWS.config.update({
        accessKeyId: parsedData.source.accessKeyId,
        secretAccessKey : parsedData.source.secretAccessKey
    });
    ECS = new AWS.ECS();
    CloudFormation = new AWS.CloudFormation();

    if(parsedData.source.stackName !== undefined) {
        stackName = parsedData.source.stackName;
        taskName = parsedData.source.taskName;
        serviceName = parsedData.source.serviceName;
        clusterName = parsedData.source.clusterName;

        runStackServiceUpdate();
    } else {
        taskArn = parsedData.source.taskName;
        serviceArn = parsedData.source.serviceName;
        clusterArn = parsedData.source.clusterName;
        runManualService();
    }
}

function runStackServiceUpdate() {
    getClusterArn()
        .then(getServiceArn)
        .then(getTaskArn)
        .then(getTaskDescription)
        .then(newTaskVersion)
        .then(updateService)
        .then(listTasks)
        .then(stopTasks)
        .then(done)
        .catch(error);
}

function runManualService() {
    getTaskDescription(taskArn)
        .then(newTaskVersion)
        .then(updateService)
        .then(listTasks)
        .then(stopTasks)
        .then(done)
        .catch(error);
}

function getClusterArn() {
    return getStackResourceARN(stackName, clusterName).then(function(data) {
        return new Promise(function(resolve, reject) {
            clusterArn = data;
            resolve(clusterArn);
        });
    });
}

function getServiceArn() {
    return getStackResourceARN(stackName, serviceName).then(function(data) {
        return new Promise(function(resolve, reject) {
            serviceArn = data
            resolve(serviceArn);
        });
    });   
}

function getTaskArn() {
    return getStackResourceARN(stackName, taskName).then(function(data) {
        return new Promise(function(resolve, reject) {
            taskArn = data;
            resolve(taskArn);
        });
    });   
}

function getStackResourceARN(stackName, logicalResourceId) {
    console.log("Getting ARN of task definition:", logicalResourceId, "from", stackName);
    var params = {
        "StackName" : stackName,
        "LogicalResourceId" : logicalResourceId
    };
    return cfPromiseMaker(
        CloudFormation.describeStackResource,
        params,
        function(data){return data.StackResourceDetail.PhysicalResourceId;},
        function(data){return "Got task description ARN: " + data.StackResourceDetail.PhysicalResourceId;}
    );
}

function printNewVersion(taskDescription) {    
    var fd3 = fs.createWriteStream(null, {fd : 3});
    fd3.write(JSON.stringify({version : { ref : taskDescription.revision.toString()}}));
}

function ecsPromiseMaker(task, params, dataTransform, log) {
    return promiseMaker(ECS, task, params, dataTransform, log);
}

function cfPromiseMaker(task, params, dataTransform, log) {
    return promiseMaker(CloudFormation, task, params, dataTransform, log);
}



function getTaskDescription(task) {
    console.log("Getting task description for:", task);
    var params = { taskDefinition : task};
    return ecsPromiseMaker(
        ECS.describeTaskDefinition,
        params,
        function(data){return data.taskDefinition;},
        function(data){return "Got task description for: " + data.taskDefinition.family;}
    );
}

function newTaskVersion(oldTask) {
    console.log("Creating new task revision from:", oldTask.family);
    var params = {
        family : oldTask.family,
        volumes: oldTask.volumes,
        containerDefinitions: oldTask.containerDefinitions
    };
    return ecsPromiseMaker(
        ECS.registerTaskDefinition,
        params,
        function(data){
            printNewVersion(data.taskDefinition);
            return data.taskDefinition;
        },
        function(data){return "Created new revision: " + data.taskDefinition.family + " : " + data.taskDefinition.revision;}
    );
}

function updateService(taskDefinition) {
    console.log("Updating service:", serviceArn, "one cluster:", clusterArn, "with revision:", taskDefinition.revision);
    var params = {
        service: serviceArn,
        cluster: clusterArn,
        taskDefinition: taskDefinition.family + ":" + taskDefinition.revision
    };
    return ecsPromiseMaker(
        ECS.updateService,
        params,
        function(data){return data;},
        function(data){return "Updated service: " + serviceArn + " with revision: " + taskDefinition.revision;}
    );
}

function listTasks() {
    params = { 
        serviceName : serviceArn,
        cluster : clusterArn
    };
    
    console.log("Listing tasks for:", params);
    return ecsPromiseMaker(
        ECS.listTasks,
        params,
        function(data){return data.taskArns;},
        function(data){return "Found tasks:" + data.taskArns;}
    );
}

function stopTasks(taskArns) {
    console.log("Stopping tasks in:", taskArns);
    return new Promise.all(taskArns.map(stopTask));
}

function stopTask(taskArn) {
    console.log("Stopping task:", taskArn);
    return ecsPromiseMaker(
        ECS.stopTask,
        {task: taskArn, cluster: clusterArn},
        function(data){return data;},
        function(data){return "Stopped task:" + taskArn;}
    );
}

function promiseMaker(taskOwner, task, params, dataTransform, log) {
    return new Promise(function(resolve, reject) {
        task.call(taskOwner, params, function(err,data) {
            
            if (err) reject(err);
            else {
                console.log(log(data));
                resolve(dataTransform(data));
            }
        });
    });
};

function removeTaskDefinitionVersion(arn) {
    return arn.substr(0, arn.lastIndexOf(":"))
}

function done() {
    console.log("DONE!");
}

function error(err) {
    console.log("ERROR", err);
    process.exit(1);
}
