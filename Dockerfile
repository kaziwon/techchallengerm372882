FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OficinaMecanica.Api.csproj ./
RUN dotnet restore OficinaMecanica.Api.csproj

COPY Application/ ./Application/
COPY Controllers/ ./Controllers/
COPY Domain/ ./Domain/
COPY Infrastructure/ ./Infrastructure/
COPY InterfaceAdapters/ ./InterfaceAdapters/
COPY Migrations/ ./Migrations/
COPY Properties/ ./Properties/
COPY Program.cs ./
COPY appsettings*.json ./

RUN dotnet publish OficinaMecanica.Api.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates gnupg wget \
    && wget -qO- https://download.newrelic.com/NEWRELIC_APT_2DAD550E.public \
        | gpg --dearmor -o /usr/share/keyrings/newrelic.gpg \
    && echo 'deb [signed-by=/usr/share/keyrings/newrelic.gpg] https://apt.newrelic.com/debian/ newrelic non-free' \
        > /etc/apt/sources.list.d/newrelic.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends newrelic-dotnet-agent=10.51.1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    CORECLR_ENABLE_PROFILING=0 \
    CORECLR_PROFILER="{36032161-FFC0-4B61-B559-F6C5D41BAE5A}" \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
    NEW_RELIC_APP_NAME=oficina-mecanica-api
EXPOSE 8080

ENTRYPOINT ["dotnet", "OficinaMecanica.Api.dll"]
