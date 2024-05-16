# Base image
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

# SDK for building and testing
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ConsoleApp1/ConsoleApp1.csproj", "ConsoleApp1/"]
RUN dotnet restore "ConsoleApp1/ConsoleApp1.csproj"

# Copy everything
COPY . .
WORKDIR "/src/ConsoleApp1"

# Test
FROM build AS test
RUN dotnet test --logger:trx

# Build
FROM test AS build-app
RUN dotnet build "ConsoleApp1.csproj" -c Release -o /app/build

# Publish
FROM build-app AS publish
RUN dotnet publish "ConsoleApp1.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables
ENV PORT=3018
ENV BOAD_CONFIG=/var/lib/app/config/board.json
ENV GAME_CONFIG=/var/lib/app/config/game.json

# Entry point
ENTRYPOINT ["dotnet", "ConsoleApp1.dll"]
