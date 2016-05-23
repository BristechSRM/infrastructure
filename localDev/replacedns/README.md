#Usage

docker run -d -v /etc/resolv.conf:/tmp/resolv.conf bristechsrm/replacedns

To specify which nameservers to add to resolv.conf, use the --dns --dns-search and --dns-opt options with docker run.
