#!/bin/bash

# Developer script to create EntityFramework database migrations
#
# $1: Name of the migration, e.g. SwitchToPermissionLevels
# Invoked manually

export LIGHTHOUSE_DB_CONNECTION_STRING='server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse'
dotnet ef migrations add "$1" --project ../ProjectLighthouse