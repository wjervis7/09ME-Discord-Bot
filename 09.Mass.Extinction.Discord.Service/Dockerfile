FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["09.Mass.Extinction.Discord.Service/09.Mass.Extinction.Discord.Service.csproj", "09.Mass.Extinction.Discord.Service/"]
RUN dotnet restore "09.Mass.Extinction.Discord.Service/09.Mass.Extinction.Discord.Service.csproj"
COPY . .
WORKDIR "/src/09.Mass.Extinction.Discord.Service"
RUN dotnet build "09.Mass.Extinction.Discord.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "09.Mass.Extinction.Discord.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "09.Mass.Extinction.Discord.Service.dll"]
