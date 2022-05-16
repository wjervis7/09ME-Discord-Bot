# Base image that runs .NET 6 app
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Build image to build .NET 6 app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["09.Mass.Extinction.Data/09.Mass.Extinction.Data.csproj", "09.Mass.Extinction.Data/"]
COPY ["09.Mass.Extinction.Discord/09.Mass.Extinction.Discord.csproj", "09.Mass.Extinction.Discord/"]
COPY ["09.Mass.Extinction.Discord.Service/09.Mass.Extinction.Discord.Service.csproj", "09.Mass.Extinction.Discord.Service/"]
RUN dotnet restore "09.Mass.Extinction.Discord.Service/09.Mass.Extinction.Discord.Service.csproj"
COPY . .
WORKDIR "/src/09.Mass.Extinction.Discord.Service"
RUN dotnet build "09.Mass.Extinction.Discord.Service.csproj" --no-restore -c Release -o /app/build

# Use build image to publish .NET 6 app
FROM build AS publish
RUN dotnet publish "09.Mass.Extinction.Discord.Service.csproj" --no-restore -c Release -o /app/publish

# Copy publish results to base image and set entry point
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "09.Mass.Extinction.Discord.Service.dll"]