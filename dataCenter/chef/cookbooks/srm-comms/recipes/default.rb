
cookbook_file '/home/ubuntu/prd.Comms.config' do
    action :create
    source 'prd.Comms.config'
    owner 'ubuntu'
    group 'ubuntu'
    mode '0444'
end

docker_image 'bristechsrm/comms' do
  action :pull
end

docker_container 'srm-comms' do
  action   :run
  repo     'bristechsrm/comms'
  port     '8080:8080'
  volumes  [ '/home/ubuntu/prd.Comms.config:/service/Comms.exe.config' ]
  command  'mono /service/Comms.exe'
end
