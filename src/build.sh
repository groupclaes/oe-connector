#!/bin/bash
version="net8.0"
configuration="Release"
docker_tag="latest"
docker_name="oe"


cd "./GroupClaes.OpenEdge.Connector/"
dotnet publish -c $configuration
rm "./bin/$configuration/$version/publish/appsettings.Development.json"
rm "./bin/$configuration/$version/publish/appsettings.json"

cd ../
docker build -t "groupclaes/${docker_name}:${docker_tag}" -f Dockerfile .
docker push "groupclaes/${docker_name}:${docker_tag}"
