#!/bin/sh

chown -R lighthouse:lighthouse /lighthouse/data

if [ -d "/lighthouse/temp" ]; then
  cp -rf /lighthouse/temp/* /lighthouse/data
  rm -rf /lighthouse/temp
fi

# run from cmd

cd /lighthouse/data
exec su-exec lighthouse:lighthouse dotnet /lighthouse/app/LBPUnion.ProjectLighthouse.Servers."$SERVER".dll

exit $? # Expose error code from dotnet command
