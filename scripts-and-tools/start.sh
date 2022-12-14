#!/bin/sh
# Startup script for production
#
# $1: Server to start; case sensitive!!!!!
# Called from systemd units

cd "$HOME"/data || (echo "Data directory not found, pls create one~" && exit 1)

echo "Running..."

# Normally this requires ASPNETCORE_URLS but we override that in the configuration
dotnet ../project-lighthouse/ProjectLighthouse.Servers."$1"/bin/Release/net7.0/LBPUnion.ProjectLighthouse.Servers."$1".dll

exit $? # Expose error code from dotnet command