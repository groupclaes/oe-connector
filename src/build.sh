#!/bin/bash
version="net7.0"
configuration="Release"
docker_tag="test"
docker_name="groupclaes_oe-connector"


cd "./GroupClaes.OpenEdge.Connector/"
dotnet publish -c $configuration
rm "./bin/$configuration/$version/publish/appsettings.Development.json"
rm "./bin/$configuration/$version/publish/appsettings.json"

cd ../
docker build -t "docker-registry.groupclaes.be/${docker_name}:${docker_tag}" -f Dockerfile .
docker push "docker-registry.groupclaes.be/${docker_name}:${docker_tag}"
