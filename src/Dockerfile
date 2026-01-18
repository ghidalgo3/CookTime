# Frontend build stage
FROM node:24-alpine AS frontend
WORKDIR /app/client-app
COPY client-app/package*.json ./
RUN npm ci
COPY client-app/ .
RUN npm run build

# Backend build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["babe-algorithms.csproj", "./"]
RUN dotnet restore "babe-algorithms.csproj"

# Copy everything else
COPY . .

# Copy frontend client build output to wwwroot
COPY --from=frontend /app/client-app/build/client/ ./wwwroot/

# Build the backend
RUN dotnet build "babe-algorithms.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "babe-algorithms.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Ensure Scripts directory is available
COPY --from=publish /src/Scripts /app/Scripts

ENTRYPOINT ["dotnet", "babe-algorithms.dll"]