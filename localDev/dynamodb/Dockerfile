FROM java:8-jre

# Pull Dynamo DB local
RUN /usr/bin/curl -L http://dynamodb-local.s3-website-us-west-2.amazonaws.com/dynamodb_local_latest | /bin/tar xz

# Install aws cli for table setup
RUN apt-get update && \
    apt-get install -y python-pip && \
    pip install --upgrade pip && \
    pip install awscli

# Default Config for aws cli + required fake access key
RUN aws configure set output json && \
    aws configure set region eu-west-1 && \
    aws configure set aws_access_key_id FAKEID && \
    aws configure set aws_secret_access_key fakeSecret

# Copy in DB table definitions
# Note fields are not specified here, only the top level table attributes.
COPY tableDefs/ tableDefs/

#Create folder for table storage
RUN mkdir /tableStore

# Create tables
RUN java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -dbPath /tableStore -sharedDb -port 7000 & \
    aws dynamodb create-table --cli-input-json file://tableDefs/auth-users-def.json --endpoint-url http://localhost:7000 && \
    aws dynamodb create-table --cli-input-json file://tableDefs/last-contact-def.json --endpoint-url http://localhost:7000 && \
    aws dynamodb create-table --cli-input-json file://tableDefs/correspondence-item-def.json --endpoint-url http://localhost:7000 && \
    aws dynamodb list-tables --endpoint-url http://localhost:7000

EXPOSE 7000

ENTRYPOINT ["java", "-Djava.library.path=./DynamoDBLocal_lib", "-jar", "DynamoDBLocal.jar", "-dbPath", "tableStore/", "-sharedDb"]
CMD ["-port", "7000"]
