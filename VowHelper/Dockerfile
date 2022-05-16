# Base image that runs .NET 6 web app
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image to build .NET 6 web app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VowHelper/VowHelper.csproj", "VowHelper/"]
RUN dotnet restore "VowHelper/VowHelper.csproj"
COPY . .
WORKDIR "/src/VowHelper"
RUN dotnet build "VowHelper.csproj" -c Release -o /app/build

# Use build image to publish .NET 6 web app
FROM build AS publish
RUN dotnet publish "VowHelper.csproj" -c Release -o /app/publish

# Node image to run NPM scripts
FROM node:17.4.0 AS node
WORKDIR /src
COPY ["VowHelper/package*.json", "VowHelper/postcss.config.js", "VowHelper/"]
COPY ["VowHelper/src", "VowHelper/src"]
WORKDIR "/src/VowHelper"
RUN npm ci
RUN npm run css
RUN npm prune --production

# Copy publish results to base image and set entry point
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node /src/VowHelper/wwwroot/css ./wwwroot/css
COPY --from=node /src/VowHelper/node_modules ./node_modules
ENTRYPOINT ["dotnet", "VowHelper.dll"]