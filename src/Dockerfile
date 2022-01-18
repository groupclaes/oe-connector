FROM docker-registry.groupclaes.be/dotnet-aspnet:latest
RUN apk add icu-libs --no-cache && rm -rf /var/cache/apk
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY GroupClaes.OpenEdge.Connector/bin/Release/net6.0/publish/ /App
WORKDIR /App
ENTRYPOINT ["dotnet", "GroupClaes.OpenEdge.Connector.dll"]