var Promise = require("promise");
var AWS = require('aws-sdk');
var lib= require('./lib');
AWS.config.update({region: 'eu-west-1'});
var ECS = new AWS.ECS();

var taskName = "create-build-image";
getTaskDescription(taskName)
    .then(newTaskVersion)
    .then(gatherServiceParams)
    .then(updateService)
    .then(listTasks)
    .then(stopTasks)
    .then(done)
    .catch(error)


function gatherServiceParams(taskRevision) {
    return findService().then(function(serviceArns) {
        return new Promise(function(resolve,reject) {
            resolve({service: serviceArns[0], task: taskRevision});
        })    
    })
}

function ecsPromiseMaker(task, params, dataTransform, log) {
    return lib.promiseMaker(ECS, task, params, dataTransform, log);
}

function getTaskDescription(taskName) {
    console.log("Getting task description for:", taskName);
    var params = { taskDefinition : taskName}
    return ecsPromiseMaker(
        ECS.describeTaskDefinition,
        params,
        function(data){return data.taskDefinition},
        function(data){return "Got task description for: " + data.taskDefinition.family}
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
        function(data){return data.taskDefinition},
        function(data){return "Created new revision: " + data.taskDefinition.family + " : " + data.taskDefinition.revision}
    );
}

function findService() {
    console.log("Finding services");
    return ecsPromiseMaker(
        ECS.listServices,
        {},
        function(data){return data.serviceArns},
        function(data){return "Found services: " + data.serviceArns}
    );
}

function updateService(serviceData) {
    console.log("Updating service:", serviceData.service, "with revision:", serviceData.task.revision);
    var params = {
        service: serviceData.service,
        taskDefinition: serviceData.task.family + ":" + serviceData.task.revision
    }
    return ecsPromiseMaker(
        ECS.updateService,
        params,
        function(data){return data},
        function(data){return "Updated service: " + serviceData.service + " with revision: " + serviceData.task.revision}
    );
}

function listTasks() {
    console.log("Listing tasks");
    return ecsPromiseMaker(
        ECS.listTasks,
        {},
        function(data){return data.taskArns},
        function(data){return "Found tasks:" + data.taskArns}
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
        function(data){return data},
        function(data){return "Stopped task:" + taskArn}
    );
}

function done() {
    console.log("DONE!");
}

function error(err) {
    console.log("ERROR", err);
    process.exit(1);
}