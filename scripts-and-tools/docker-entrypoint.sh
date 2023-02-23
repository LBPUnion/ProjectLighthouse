#!/bin/sh

log() {
    local type="$1"; shift
    printf '%s [%s] [Entrypoint]: %s\n' "$(date -Iseconds)" "$type" "$*"
}

log Note "Entrypoint script for Lighthouse $SERVER started".

if [ ! -d "/lighthouse/data" ]; then
    log Note "Creating data directory"
    mkdir -p "/lighthouse/data"
    chown -R lighthouse:lighthouse /lighthouse/data
fi

if [ -d "/lighthouse/temp" ]; then
    log Note "Copying temp directory to data"
    cp -rf /lighthouse/temp/* /lighthouse/data
    rm -rf /lighthouse/temp
fi

# run from cmd

log Note "Startup tasks finished, starting $SERVER..."
cd /lighthouse/data
exec su-exec lighthouse:lighthouse dotnet /lighthouse/app/LBPUnion.ProjectLighthouse.Servers."$SERVER".dll

exit $? # Expose error code from dotnet command