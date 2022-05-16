#!/bin/bash

export LIGHTHOUSE_DB_CONNECTION_STRING='server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse'
dotnet ef migrations add "$1" --project ProjectLighthouse