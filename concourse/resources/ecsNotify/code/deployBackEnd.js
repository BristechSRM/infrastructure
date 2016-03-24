var fs = require('fs');
var Promise = require("promise");
var AWS = require('aws-sdk');
AWS.config.update({region: 'eu-west-1'});
var ECS;
var CloudFormation;

var taskName;
var stackName;

var stdin = process.stdin, inputChunks = [];
stdin.resume();
stdin.setEncoding('utf8');
stdin.on('data', function(chunk) {
    inputChunks.push(chunk);
});
stdin.on('end', readConfig);

function readConfig() {
    var parsedData = JSON.parse(inputChunks.join());

    taskName = parsedData.source.task;
    stackName = parsedData.source.stackName;
    AWS.config.update({
        accessKeyId: parsedData.source.accessKeyId,
        secretAccessKey : parsedData.source.secretAccessKey
    });
    ECS = new AWS.ECS();
    CloudFormation = new AWS.CloudFormation();

    runDasCode();

}

function runDasCode() {
    getTaskDescriptionARN(stackName, taskName)
        .then(getTaskDescription)
        .then(newTaskVersion)
        .then(gatherServiceParams)
        .then(updateService)
        .then(listTasks)
        .then(stopTasks)
        .then(done)
        .catch(error);
}

function gatherServiceParams(taskRevision) {
    var fd3 = fs.createWriteStream(null, {fd : 3});
    fd3.write(JSON.stringify({version : { ref : taskRevision.revision.toString()}}))

    return findService().then(function(serviceArns) {
        return new Promise(function(resolve,reject) {
            resolve({service: serviceArns[0], task: taskRevision});
        });
    });
}

function ecsPromiseMaker(task, params, dataTransform, log) {
    return promiseMaker(ECS, task, params, dataTransform, log);
}

function cfPromiseMaker(task, params, dataTransform, log) {
    return promiseMaker(CloudFormation, task, params, dataTransform, log);
}

function noOpPromise(returner) {
    return new Promise(function (reject, resolve) {
        resolve(returner);
    })
}

function getTaskDescriptionARN(stackName, logicalResourceId) {
    if(stackName !== undefined){
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

    } else {
        return noOpPromise(logicalResourceId);
    }
}

function getTaskDescription(taskName) {
    console.log("Getting task description for:", taskName);
    var params = { taskDefinition : taskName};
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
        function(data){return data.taskDefinition;},
        function(data){return "Created new revision: " + data.taskDefinition.family + " : " + data.taskDefinition.revision;}
    );
}

function findService() {
    console.log("Finding services");
    return ecsPromiseMaker(
        ECS.listServices,
        {},
        function(data){return data.serviceArns;},
        function(data){return "Found services: " + data.serviceArns;}
    );
}

function updateService(serviceData) {
    console.log("Updating service:", serviceData.service, "with revision:", serviceData.task.revision);
    var params = {
        service: serviceData.service,
        taskDefinition: serviceData.task.family + ":" + serviceData.task.revision
    };
    return ecsPromiseMaker(
        ECS.updateService,
        params,
        function(data){return data;},
        function(data){return "Updated service: " + serviceData.service + " with revision: " + serviceData.task.revision;}
    );
}

function listTasks() {
    console.log("Listing tasks");
    return ecsPromiseMaker(
        ECS.listTasks,
        {},
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
        {task: taskArn},
        function(data){return data;},
        function(data){return "Stopped task:" + taskArn;}
    );
}

function promiseMaker(taskOwner, task, params, dataTransform, log) {
    return new Promise(function(resolve, reject) {
        // console.log(task, taskOwner, params)
        task.call(taskOwner, params, function(err,data) {
            console.log(err, data);
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
