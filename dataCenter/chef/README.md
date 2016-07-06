Chef recipes
========================

Briefly, as if for bert at Chickens, Inc.


On the server
------------------------

```
    $ sudo su
    $ cd /etc/opscode
````
edit chef-server.rb and insert the following
```
server_name = "<serverfqdn>"
api_fqdn server_name
bookshelf['vip'] = server_name
nginx['url'] = "https://#{server_name}"
nginx['server_name'] = server_name
nginx['ssl_certificate'] = "/var/opt/opscode/nginx/ca/#{server_name}.crt"
nginx['ssl_certificate_key'] = "/var/opt/opscode/nginx/ca/#{server_name}.key"
```
then reconfigure and create your admin user and organisation
```
    $ chef-server-ctl reconfigure

    $ chef-server-ctl user-create bert Bert Chicken bert@chickens.com abetterpasswordthanthis --filename bert.pem

    $ chef-server-ctl org-create chickens "Chickens with typewriters, Inc." --association_user bert --filename chickens.pem
```

On the node
------------------------

```
    $ sudo su
    $ mkdir /etc/chef
    $ cd /etc/chef
```
copy the chickens.pem file from the server to /etc/chef

edit client.rb and insert the following
```
log_level              :info
log_location           STDOUT
chef_server_url        'https://<serverfqdn>/organizations/chickens'
validation_client_name 'chickens-validator'
validation_key         '/etc/chef/chickens.pem'
client_key             '/etc/chef/client.pem'
ssl_verify_mode        :verify_none
```
and then run the client to get a unique client.pem:
```
    $ chef-client
```
and delete the chickens.pem
```
    $ rm chickens.pem
```

On your workstation
------------------------

Install the ChefDK

Make sure ssh is in your %PATH% (various tools, e.g. GIT include it)

copy the bert.pem file to e.g. C:\\Users\\bert\\.chef

edit C:\\Users\\bert\\.chef\\knife.rb and insert the following
```
log_level                :info
log_location             STDOUT
chef_server_url          'https://<serverfqdn>/organizations/chickens'
client_key               'C:/Users/bert/.chef/bert.pem'
node_name                'bert'
syntax_check_cache_path  'C:/Users/bert/.chef/syntax_check_cache'
cookbook_path            [ '.', 'C:/Users/bert/.berkshelf/cookbooks' ]
```
and then fetch the server certificate
```
    > knife ssl fetch
```
check connectivity with
```
    > knife node list
```
Use Berkshelf to get dependencies locally
```
    > berks install
```
Then load the cookbook with dependencies to the server, and set the run_list for the node
```
    > knife cookbook upload srm-docker --include-dependencies

    > knife cookbook upload srm-comms

    > knife node run_list set <nodefqdn> 'apt::default,apt-docker::default,srm-docker::default'
```

Now, on the node run chef-client
```
    > sudo chef-client
```
