# Build stage for CLI project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CLI/CLI.fsproj", "CLI/"]
COPY ["Shared/Shared.fsproj", "Shared/"]
COPY CLI/. ./CLI/
COPY Shared/. ./Shared/
RUN dotnet restore "CLI/CLI.fsproj"
RUN dotnet build "CLI/CLI.fsproj" -c Release -o /app/build
RUN dotnet publish "CLI/CLI.fsproj" -c Release -o /app/publish /p:UseAppHost=false

# Runner stage for CLI project
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CLI.dll"]
