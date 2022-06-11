#!/bin/sh
# Update script for production
#
# No arguments
# Called manually

sudo systemctl stop project-lighthouse*

cd /srv/lighthouse || return
sudo -u lighthouse -i /srv/lighthouse/build.sh

sudo systemctl start project-lighthouse*