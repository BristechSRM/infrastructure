# Database Infrastructure

## Initial Notes
Currently only db-create.sql script under 0. 
Once system is live and database version upgrades need to happen, Either investigate switching to a full framework,
or simply start using version folders with up and down scripts sets for each version.  

## Database restoration
### Dynamo Db
The DynamoDb databases are managed, so no restoration wil be required. 
### MySql Db 
The mysql databases are scheduled to backup once a day, and backups are kept for one day. 
To as part of the backup execution, snapshots are taken. These have a history of 3 days. 

When performing the restoration, make sure that everything is configured as before. THe most important things are 
 - Cheaper instances and storage type (db.t2.micro and magnetic is used at the moment.)
 - Single AZ Delopyment
 - Correct Network and security settings (See current databases)
 - default port

If possible, don't delete the old database until the restoration is complete, and use it as a reference for settings. 

To perform any restoration, follow the aws documentation. One link is : [http://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_CommonTasks.BackupRestore.html](http://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_CommonTasks.BackupRestore.html).
Other references: 
[Google Search- backup vs snapshot](https://www.google.co.uk/search?q=aws+rds+backup+vs+snapshot&ie=utf-8&oe=utf-8&client=firefox-b&gfe_rd=cr&ei=MDvuV8DTIO7v8Aejh7HYCA)