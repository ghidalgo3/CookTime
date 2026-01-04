# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["babe-algorithms.csproj", "./"]
RUN dotnet restore "babe-algorithms.csproj"

# Copy everything else and build
COPY . .
# RUN dotnet build "babe-algorithms.csproj" -c Release -o /app/build

# # Publish stage
# FROM build AS publish
# RUN dotnet publish "babe-algorithms.csproj" -c Release -o /app/publish /p:UseAppHost=false

# # Runtime stage
# FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# WORKDIR /app

# COPY --from=publish /app/publish .

# # Ensure Scripts directory is available
# COPY Scripts /app/Scripts