# Infrastructure

## CI with Concourse

An unsecured Concourse Lite instance can be set up on AWS using the Vagrantfile under /concourse.
You will need to replace <insert here> with your AWS access key ID and secret access key.
You may also need to set the AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY environment variables in the shell where you run "vagrant up --provider=aws".
