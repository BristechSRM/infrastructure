
docker_installation_package 'default' do
  action :create
  version '1.11.1'
  package_options %q|--force-yes -o Dpkg::Options::='--force-confold' -o Dpkg::Options::='--force-all'|
end

docker_service_manager 'default' do
  action :start
  debug true
  host [ 'tcp://127.0.0.1:2375', 'unix:///var/run/docker.sock' ]
end
