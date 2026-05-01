FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OficinaMecanica.Api/OficinaMecanica.Api.csproj OficinaMecanica.Api/
RUN dotnet restore OficinaMecanica.Api/OficinaMecanica.Api.csproj

COPY . .
RUN dotnet publish OficinaMecanica.Api/OficinaMecanica.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OficinaMecanica.Api.dll"]
