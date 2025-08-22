FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy solution file
COPY *.sln ./

# Copy project files
COPY src/*.csproj ./src/
COPY tests/HomeAutomation.Tests/*.csproj ./tests/HomeAutomation.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ ./src/
COPY tests/ ./tests/

# Build and publish
RUN dotnet publish -c Release -o out ./src/HomeAutomation.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "HomeAutomation.dll"]