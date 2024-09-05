#!/bin/bash
version="net7.0"
configuration="Release"
docker_tag="test"
docker_name="oe-connector"


cd "./GroupClaes.OpenEdge.Connector/"
dotnet publish -c $configuration
rm "./bin/$configuration/$version/publish/appsettings.Development.json"
rm "./bin/$configuration/$version/publish/appsettings.json"

cd ../
docker build -t "groupclaes/${docker_name}:${docker_tag}" -f Dockerfile .
docker push "groupclaes/${docker_name}:${docker_tag}"
