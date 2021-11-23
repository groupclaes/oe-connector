SET dotnet_version=net5.0
SET docker_tag=latest
SET docker_name=groupclaes_oe-connector
SET docker_registry=docker-registry.groupclaes.be



cd ".\GroupClaes.OpenEdge.Connector\"
dotnet publish -c Release
DEL /F ".\bin\Release\%dotnet_version%\publish\appsettings.Development.json"
:: DEL /F ".\bin\Release\%dotnet_version%\publish\appsettings.json"

cd ../
::docker build -t "%docker_registry%/%docker_name%:%docker_tag%" -f Dockerfile .
::docker push "%docker_registry%/%docker_name%:%docker_tag%"
