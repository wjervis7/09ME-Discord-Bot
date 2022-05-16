# Base image that runs .NET 6 web app
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image to build .NET 6 web app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["09.Mass.Extinction.Data/09.Mass.Extinction.Data.csproj", "09.Mass.Extinction.Data/"]
COPY ["09.Mass.Extinction.Discord/09.Mass.Extinction.Discord.csproj", "09.Mass.Extinction.Discord/"]
COPY ["09.Mass.Extinction.Web/09.Mass.Extinction.Web.csproj", "09.Mass.Extinction.Web/"]
RUN dotnet restore "09.Mass.Extinction.Web/09.Mass.Extinction.Web.csproj"
COPY . .
WORKDIR "/src/09.Mass.Extinction.Web"
RUN dotnet build "09.Mass.Extinction.Web.csproj" --no-restore -c Release -o /app/build

# Use build image to publish .NET 6 web app
FROM build AS publish
RUN dotnet publish "09.Mass.Extinction.Web.csproj" --no-restore -c Release -o /app/publish

# Node image to run NPM scripts
FROM node:17.4.0 AS node
WORKDIR /src
COPY ["09.Mass.Extinction.Web/package*.json", "09.Mass.Extinction.Web/postcss.config.js", "09.Mass.Extinction.Web/"]
COPY ["09.Mass.Extinction.Web/src", "09.Mass.Extinction.Web/src"]
WORKDIR "/src/09.Mass.Extinction.Web"
RUN npm i
RUN npm run css

# Copy publish results to base image and set entry point
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node /src/09.Mass.Extinction.Web/wwwroot/css ./wwwroot/css
COPY --from=node /src/09.Mass.Extinction.Web/wwwroot/fonts ./wwwroot/fonts
ENTRYPOINT ["dotnet", "09.Mass.Extinction.Web.dll"]
