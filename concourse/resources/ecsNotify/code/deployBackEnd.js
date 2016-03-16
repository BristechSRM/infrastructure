var Promise = require("promise");
var AWS = require('aws-sdk');
AWS.config.update({region: 'eu-west-1'});
var ECS;

var taskName;

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
    AWS.config.update({
        accessKeyId: parsedData.source.accessKeyId,
        secretAccessKey : parsedData.source.secretAccessKey
    });
    ECS = new AWS.ECS();

    runDasCode();
}

function runDasCode() {
    getTaskDescription(taskName)
        .then(newTaskVersion)
        .then(gatherServiceParams)
        .then(updateService)
        .then(listTasks)
        .then(stopTasks)
        .then(done)
        .catch(error);
}

function gatherServiceParams(taskRevision) {
    console.log(JSON.stringify({version : { ref : taskRevision.revision.toString()}}))
    return findService().then(function(serviceArns) {
        return new Promise(function(resolve,reject) {
            resolve({service: serviceArns[0], task: taskRevision});
        });
    });
}

function ecsPromiseMaker(task, params, dataTransform, log) {
    return promiseMaker(ECS, task, params, dataTransform, log);
}

function getTaskDescription(taskName) {
    console.error("Getting task description for:", taskName);
    var params = { taskDefinition : taskName};
    return ecsPromiseMaker(
        ECS.describeTaskDefinition,
        params,
        function(data){return data.taskDefinition;},
        function(data){return "Got task description for: " + data.taskDefinition.family;}
    );
}

function newTaskVersion(oldTask) {
    console.error("Creating new task revision from:", oldTask.family);
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
    console.error("Finding services");
    return ecsPromiseMaker(
        ECS.listServices,
        {},
        function(data){return data.serviceArns;},
        function(data){return "Found services: " + data.serviceArns;}
    );
}

function updateService(serviceData) {
    console.error("Updating service:", serviceData.service, "with revision:", serviceData.task.revision);
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
    console.error("Listing tasks");
    return ecsPromiseMaker(
        ECS.listTasks,
        {},
        function(data){return data.taskArns;},
        function(data){return "Found tasks:" + data.taskArns;}
    );
}

function stopTasks(taskArns) {
    console.error("Stopping tasks in:", taskArns);
    return new Promise.all(taskArns.map(stopTask));
}

function stopTask(taskArn) {
    console.error("Stopping task:", taskArn);
    return ecsPromiseMaker(
        ECS.stopTask,
        {task: taskArn},
        function(data){return data;},
        function(data){return "Stopped task:" + taskArn;}
    );
}

function promiseMaker(taskOwner, task, params, dataTransform, log) {
    return new Promise(function(resolve, reject) {
        task.call(taskOwner, params, function(err,data) {
            if (err) reject(err);
            else {
                console.error(log(data));
                resolve(dataTransform(data));
            }
        });
    });
};

function done() {
    console.error("DONE!");
}

function error(err) {
    console.error("ERROR", err);
    process.exit(1);
}
