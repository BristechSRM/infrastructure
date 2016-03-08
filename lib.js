var Promise = require("promise");

var promiseMaker = function(taskOwner, task, params, dataTransform, log) {
    return new Promise(function(resolve, reject) {
        task.call(taskOwner, params, function(err,data) {
            if (err) reject(err);
            else {
                console.log(log(data));
                resolve(dataTransform(data));
            }
        });
    })
}


module.exports.promiseMaker = promiseMaker;
