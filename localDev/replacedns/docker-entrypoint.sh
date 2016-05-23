#!/bin/sh
backupDns() {
	cp /tmp/resolv.conf /tmp/resolv.conf.backup
}

replaceDns() {
	printf "%b" "$(cat /etc/resolv.conf)\n" > /tmp/resolv.conf
}

restoreDns() {
	printf "%b" "$(cat /tmp/resolv.conf.backup)\n" > /tmp/resolv.conf
	rm /tmp/resolv.conf.backup
	exit
}

trap restoreDns INT TERM

backupDns
replaceDns

while true; do
	sleep 5 # Do nothing
done
