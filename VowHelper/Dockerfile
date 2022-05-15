#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VowHelper/VowHelper.csproj", "VowHelper/"]
RUN dotnet restore "VowHelper/VowHelper.csproj"
COPY . .
WORKDIR "/src/VowHelper"
RUN dotnet build "VowHelper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VowHelper.csproj" -c Release -o /app/publish

# Node image to run NPM scripts
FROM node:17.4.0 AS node
WORKDIR /src
COPY ["VowHelper/package*.json", "VowHelper/postcss.config.js", "VowHelper/"]
COPY ["VowHelper/src", "VowHelper/src"]
WORKDIR "/src/09.Mass.Extinction.Web"
RUN npm install
RUN npm run css

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VowHelper.dll"]