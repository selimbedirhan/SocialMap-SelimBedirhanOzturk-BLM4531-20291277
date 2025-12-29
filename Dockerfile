# Backend Multi-stage Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY *.sln ./
COPY SocialMap.Core/*.csproj ./SocialMap.Core/
COPY SocialMap.Business/*.csproj ./SocialMap.Business/
COPY SocialMap.Repository/*.csproj ./SocialMap.Repository/
COPY SocialMap.WebAPI/*.csproj ./SocialMap.WebAPI/

# Restore packages
RUN dotnet restore

# Copy source code
COPY . .

# Build application
WORKDIR /src/SocialMap.WebAPI
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Copy published app
COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

# Environment variables
ENV ASPNETCORE_URLS=http://+:5280
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5280/health || exit 1

EXPOSE 5280

ENTRYPOINT ["dotnet", "SocialMap.WebAPI.dll"]
