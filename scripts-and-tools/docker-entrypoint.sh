#!/bin/sh

chown -R lighthouse:lighthouse /lighthouse/data

if [ -d "/lighthouse/temp" ]; then
  cp -rf /lighthouse/temp/* /lighthouse/data
  rm -rf temp
fi
su -s /bin/sh -l lighthouse

# run from cmd

cd /lighthouse/data && dotnet /lighthouse/app/LBPUnion.ProjectLighthouse.Servers."$SERVER".dll

exit $? # Expose error code from dotnet command
