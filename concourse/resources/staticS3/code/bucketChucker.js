var Promise = require("promise");
var AWS = require('aws-sdk');
var fs = require('fs');
var mime = require('mime');

AWS.config.update({region: 'eu-west-1'});

var args = process.argv.slice(2);
var inputDir = args[0];

console.error("Reading from:", inputDir)
var buildReader = createBuildReader(inputDir);

var s3;
var inputBucket;
var inputParams;
var buildUploader;

var stdin = process.stdin, inputChunks = [];
stdin.resume();
stdin.setEncoding('utf8');
stdin.on('data', function(chunk) {
    inputChunks.push(chunk);
});
stdin.on('end', readConfig);

function readConfig() {
    var parsedData = JSON.parse(inputChunks.join());

    inputBucket = parsedData.source.bucket;
    AWS.config.update({
        accessKeyId: parsedData.source.accessKeyId,
        secretAccessKey : parsedData.source.secretAccessKey
    });
    s3 = new AWS.S3();
    buildUploader = createFileUploader(inputBucket, inputDir);

    //Empty string as this starts at the route of the build directory
    buildReader("").catch(error);
}

function createBuildReader(dir) {
    return function(subDir) {
        return readDirectory(dir, subDir).then(uploadDirectory);
    };
}

function readDirectory(baseDir, subDir) {
    console.error("Reading Directory:", baseDir, subDir);
    return promiseMaker(
        fs,
        fs.readdir,
        joinPaths(baseDir,subDir),
        function(data) {return {base: baseDir, sub: subDir, files:data};},
        function(data) {return "Read " + data.length + " files from build directory:  "  + data;}
    );
}

function uploadDirectory(dir) {
    console.error("Starting upload from:", dir.base, dir.sub);
    var directoryProccessor = createDirectoryProccessor(dir);
    return new Promise.all(dir.files.map(directoryProccessor));
}

function createDirectoryProccessor(dir) {
    return function(fileName) {
        return proccessDirectory(dir, fileName);
    };
}

function proccessDirectory(dir, fileName) {
    var file = fs.lstatSync(dirToPath(dir,fileName));
    var longName =  joinPaths(dir.sub, fileName);

    if(file.isDirectory()){
        return buildReader(longName);
    } else {
        return buildUploader(longName);
    }
}

function createFileUploader(bucket, baseDir) {
    return function(fileName) {
        return uploadFile(bucket, baseDir, fileName);
    };
}

function uploadFile(bucket, dir, fileName) {
    console.error("Uploading: ", bucket, dir, fileName);
    var filePath = dir + "/" + fileName;
    var file = require('fs').createReadStream(filePath);
    var mimeType = mime.lookup(joinPaths(dir, fileName));
    var params = {Bucket: bucket, Key: fileName, Body: file, ContentType: mimeType};
    return promiseMaker(
        s3,
        s3.upload,
        params,
        function(data) {return data;},
        function(data) {return "Finished Uploading: "  + data.key;}
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
}

function joinPaths(baseDir, subDir) {
    if(/.*[^\/]$/.test(baseDir)){
        baseDir += "/";
    }
    return baseDir + subDir;
}

function dirToPath(dir, file){
    return dir.base + "/" + dir.sub + "/" + file;
}

function error(err) {
    console.error("ERROR", err);
    process.exit(1);
}
