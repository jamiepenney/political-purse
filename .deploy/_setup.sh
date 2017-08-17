#!/bin/bash
set -e
cd $( dirname "${BASH_SOURCE[0]}" )

# Get ssh connection info
SSH_SERVER=`cat .server`

echo -e "\e[1m\e[34mSetup production configs\e[0m"
ssh "${SSH_SERVER}" "mkdir -p /var/app/config"
scp -rp ./config/. "${SSH_SERVER}:/var/app/config"

echo -e "\e[1m\e[34mSetup nginx\e[0m"
scp -rp ./nginx/. "${SSH_SERVER}:/etc/nginx/sites-available"
ssh "${SSH_SERVER}" "cd /etc/nginx/sites-enabled && (([ -e default ] && rm default) || true) && (([ -e politicalpurse ] && rm politicalpurse) || true) && sudo ln -s ../sites-available/politicalpurse politicalpurse && sudo ln -s ../sites-available/default default && sudo nginx -s reload"

echo -e "\e[1m\e[34mSetup dotnet service\e[0m"
scp -rp ./service/. "${SSH_SERVER}:/etc/systemd/system/"
ssh "${SSH_SERVER}" "systemctl enable politicalpurse.service"
