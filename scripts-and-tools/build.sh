#!/bin/sh
# Build script for production
#
# No arguments
# Called manually

cd project-lighthouse || (echo "Source directory not found, pls clone properly~" && exit 1)

echo "Pulling latest changes..."
git pull

echo "Building..."
dotnet build -c Release

exit $? # Expose error code from build command