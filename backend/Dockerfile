# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Pharos.Api/Pharos.Api.csproj", "Pharos.Api/"]
COPY ["Pharos.Core/Pharos.Core.csproj", "Pharos.Core/"]
RUN dotnet restore "Pharos.Api/Pharos.Api.csproj"
COPY . .
WORKDIR "/src/Pharos.Api"
RUN dotnet build "Pharos.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Pharos.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pharos.Api.dll"]
