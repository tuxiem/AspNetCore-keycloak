# use alpine for smaller footprint
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base

# Environment variables are exposed automatic in application.json
# Keycloak__Metadata
# Docker run -e Keycloak__Metadata="something" containerName

# Needed to get alpine to work
RUN apk add --no-cache tzdata
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5001
# EXPOSE 5000 - for http only
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

WORKDIR /src
COPY ["/KeycloakAuth/KeycloakAuth.csproj", "KeycloakAuth/"]
RUN dotnet restore "KeycloakAuth/KeycloakAuth.csproj"
COPY . .
WORKDIR "/src/KeycloakAuth"
RUN dotnet build "KeycloakAuth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KeycloakAuth.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KeycloakAuth.dll"]