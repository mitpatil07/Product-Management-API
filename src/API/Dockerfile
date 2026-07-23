# Multi-stage build Dockerfile for RESTful Product Management API (.NET 8)

# 1. Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8080

# 2. SDK build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/API/API.csproj", "src/API/"]

RUN dotnet restore "src/API/API.csproj"

# Copy all source files and build
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# 3. Publish the application
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
