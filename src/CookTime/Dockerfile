FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN apt-get update && apt-get install -y \
    ca-certificates \
    curl \
    apt-transport-https \
    lsb-release \
    gnupg && \
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy csproj and restore dependencies
COPY ["CookTime.csproj", "./"]
RUN dotnet restore "CookTime.csproj"

# Copy everything else
COPY . .

# Build the backend
RUN dotnet build "CookTime.csproj" -c Debug -o /app/build