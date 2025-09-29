# Build stage for API project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["API/API.fsproj", "API/"]
COPY ["Shared/Shared.fsproj", "Shared/"]
COPY API/. ./API/
COPY Shared/. ./Shared/
RUN dotnet restore "API/API.fsproj"
RUN dotnet build "API/API.fsproj" -c Release -o /app/build
RUN dotnet publish "API/API.fsproj" -c Release -o /app/publish /p:UseAppHost=false

# Runner stage for API project
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
