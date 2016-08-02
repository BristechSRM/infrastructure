# Db Only Docker Compose
Run `./up.sh` to bring up a blank local instance of DynamoDb and MySql. 
The Mysql database will be setup with the required tables for BristechSRM.

DynamoDb will be available at `http://localhost:7000`. To use with the comms service, for example, In the comms app.config set 
`LocalDynamodBUrl` to `http://localhost:7000`

MySql will be available at `http://localhost:3306`. The database is called `sessions`, the user is `root` and the password is `localdev`. 
To use with the sessions service, set your connection string to 
`SERVER=localhost;Database=sessions;UID=root;Password=localdev;`

Do not use these for anything other than local development, and do not expose to the internet! 
