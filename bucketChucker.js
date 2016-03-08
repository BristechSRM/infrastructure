var Promise = require("promise");
var AWS = require('aws-sdk');
var fs = require('fs');
var lib = require('./lib');
AWS.config.update({region: 'eu-west-1'});
var s3 = new AWS.S3();

var args = process.argv.slice(2);
var inputDir = args[0]
var inputBucket = args[1]

readBuild(inputDir)
	.then(uploadFiles);


function readBuild(dir) {
	console.log("Reading Directory " + dir);
	return lib.promiseMaker(
		fs, 
		fs.readdir,
		dir,
		function(data) {return data},
		function(data) {return "Read " + data.length + " files from build directory: ", data}
	)
}

function uploadFiles(files) {
	console.log("Starting upload of files:" + files)
	return new Promise.all(files.map(uploadMetaWrapper));
}

function uploadMetaWrapper(file) {
	return uploadFile(inputBucket, inputDir, file);
}


function uploadFile(bucket, dir, fileName) {
	console.log("Uploading: ", bucket, dir, fileName)
	var file = require('fs').createReadStream(dir + "/" + fileName);
	var params = {Bucket: bucket, Key: fileName, Body: file};
	return lib.promiseMaker(
		s3, 
		s3.upload,
		params,
		function(data) {return data},
		function(data) {return "Finished Uploading: "  + data.key}
	)

	s3.upload(params, function(err,data){console.log(err,data)});
}