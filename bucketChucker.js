var Promise = require("promise");
var AWS = require('aws-sdk');
var fs = require('fs');
var mime = require('mime');
var lib = require('./lib');

AWS.config.update({region: 'eu-west-1'});
var s3 = new AWS.S3();

var args = process.argv.slice(2);
var inputDir = args[0];
var inputBucket = args[1];

var buildReader = createBuildReader(inputDir);
var buildUploader = createFileUploader(inputBucket, inputDir);
buildReader("").catch(error);

function createBuildReader(dir) {
    return function(subDir) {
        return readDirectory(dir, subDir).then(uploadDirectory);
    };
}

function readDirectory(baseDir, subDir) {
    console.log("Reading Directory:", baseDir, subDir);
    return lib.promiseMaker(
        fs,
        fs.readdir,
        joinPaths(baseDir,subDir),
        function(data) {return {base: baseDir, sub: subDir, files:data};},
        function(data) {return "Read " + data.length + " files from build directory:  "  + data;}
    );
}

function uploadDirectory(dir) {
    console.log("Starting upload from:", dir.base, dir.sub);
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
    console.log("Uploading: ", bucket, dir, fileName);
    var filePath = dir + "/" + fileName;
    var file = require('fs').createReadStream(filePath);
    var mimeType = mime.lookup(joinPaths(dir, fileName));
    var params = {Bucket: bucket, Key: fileName, Body: file, ContentType: mimeType};
    return lib.promiseMaker(
        s3,
        s3.upload,
        params,
        function(data) {return data;},
        function(data) {return "Finished Uploading: "  + data.key;}
    );
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
    console.log("ERROR", err);
    process.exit(1);
}
