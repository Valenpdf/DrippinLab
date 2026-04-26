# Multi-stage build for Drippin - Optimized for smaller image
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file and restore (optimizes layer caching)
COPY Drippin.csproj ./Drippin.csproj
RUN dotnet restore Drippin.csproj

# Copy all source files
COPY . .

# Build and publish
RUN dotnet publish Drippin.csproj -c Release -o /src/publish --no-restore

# Stage 2: Runtime (smaller than full aspnet image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime

WORKDIR /app

# Copy published files from build stage
COPY --from=build /src/publish /app

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser && \
    chown -R appuser:appgroup /app
USER appuser

# Expose port 80
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "/app/Drippin.dll"]