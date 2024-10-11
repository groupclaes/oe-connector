FROM groupclaes/dotnet-aspnet:8.0
RUN apk add icu-libs --no-cache && rm -rf /var/cache/apk
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY GroupClaes.OpenEdge.Connector/bin/Release/net8.0/publish/ /App
WORKDIR /App
ENTRYPOINT [ "dotnet", "GroupClaes.OpenEdge.Connector.dll" ]