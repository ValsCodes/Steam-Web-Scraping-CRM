# Build context: SteamApp.Server
# Example:
#   docker build -f SteamApp.WebAPI/Dockerfile -t steamapp-server:local .

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["SteamApp.WebAPI/SteamApp.WebAPI.csproj", "SteamApp.WebAPI/"]
COPY ["SteamApp.Infrastructure/SteamApp.Infrastructure.csproj", "SteamApp.Infrastructure/"]
COPY ["SteamApp.Interfaces/SteamApp.Interfaces.csproj", "SteamApp.Interfaces/"]
COPY ["SteamApp.Application/SteamApp.Application.csproj", "SteamApp.Application/"]
COPY ["SteamApp.Models/SteamApp.Domain.csproj", "SteamApp.Models/"]
RUN dotnet restore "SteamApp.WebAPI/SteamApp.WebAPI.csproj"

COPY . .
WORKDIR "/src/SteamApp.WebAPI"
RUN dotnet build "SteamApp.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SteamApp.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "SteamApp.WebAPI.dll"]
