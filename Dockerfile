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

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

ENTRYPOINT ["dotnet", "OficinaMecanica.Api.dll"]
