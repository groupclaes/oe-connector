FROM mcr.microsoft.com/dotnet/aspnet:5.0-windowsservercore-ltsc2019
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY GroupClaes.OpenEdge.Connector/bin/Release/net5.0/publish/ c:/App
WORKDIR c:/App
ENTRYPOINT ["dotnet", "GroupClaes.OpenEdge.Connector.dll"]