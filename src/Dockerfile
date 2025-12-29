# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Install Node.js and npm
RUN apt-get update && apt-get install -y \
    curl \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

# Copy csproj and restore dependencies
COPY ["babe-algorithms.csproj", "./"]
RUN dotnet restore "babe-algorithms.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "babe-algorithms.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "babe-algorithms.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Ensure Scripts directory is available
COPY Scripts /app/Scripts

EXPOSE 5000

ENTRYPOINT ["dotnet", "babe-algorithms.dll"]
