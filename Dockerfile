FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EasyLog/EasyLog.csproj EasyLog/
COPY EasySave.Core/EasySave.Core.csproj EasySave.Core/
COPY EasySave.Console/EasySave.Console.csproj EasySave.Console/
RUN dotnet restore EasySave.Console/EasySave.Console.csproj

COPY EasyLog/ EasyLog/
COPY EasySave.Core/ EasySave.Core/
COPY EasySave.Console/ EasySave.Console/
RUN dotnet publish EasySave.Console/EasySave.Console.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

RUN mkdir -p /data/source /data/target /data/logs /data/config

COPY --from=build /app/publish .

ENV EASYSAVE_CONFIG=/data/config/config.json
ENV EASYSAVE_LOGS=/data/logs

VOLUME ["/data"]

ENTRYPOINT ["dotnet", "EasySave.Console.dll"]
