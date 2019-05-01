#!/bin/bash
set -e
cd $( dirname "${BASH_SOURCE[0]}" )

# Get ssh connection info
SSH_SERVER=`cat .server`

# date time stamp function
dts() { date +%Y%m%d%H%M%S; }

cd ../PoliticalPurse.Web

echo -e "\e[1m\e[34mBuild the javascript and css\e[0m"
npm install --production --progress false && npm run build

echo -e "\e[1m\e[34mBuild the dotnet app\e[0m"
dotnet publish -v m -c release

# Remove the appsettings.json file if it already exists, the server one should take pref
([ -e ./bin/Release/netcoreapp2.1/publish/appsettings.json ] && rm ./bin/Release/netcoreapp2.1/publish/appsettings.json)

# Create a new timestamp named directory for the deployment
deploy_dir="/var/app/$(dts)"

echo -e "\e[1m\e[34mCopy web app to server dir ${deploy_dir}\e[0m"
ssh "${SSH_SERVER}" "mkdir -p ${deploy_dir} && chown :www-data ${deploy_dir}"

scp -rp ./bin/Release/netcoreapp2.0/publish/. "${SSH_SERVER}:${deploy_dir}"

echo -e "\e[1m\e[34mLink configs\e[0m"
ssh "${SSH_SERVER}" "cd ${deploy_dir} && sudo ln -s ../config/appsettings.json appsettings.json  && systemctl restart politicalpurse.service"

echo -e "\e[1m\e[34mRestart server\e[0m"
ssh "${SSH_SERVER}" "cd /var/app && ([ -e current ] && rm current) || true && sudo ln -s ${deploy_dir} current && systemctl restart politicalpurse.service"

cd ..

