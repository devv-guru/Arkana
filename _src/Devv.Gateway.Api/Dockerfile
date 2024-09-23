# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Devv.Gateway.Data/Devv.Gateway.Data.csproj", "Devv.Gateway.Data/"]
COPY ["Devv.Gateway.Api/Devv.Gateway.Api.csproj", "Devv.Gateway.Api/"]

RUN dotnet restore "Devv.Gateway.Api/Devv.Gateway.Api.csproj"

# Move this line here to optimize build caching
COPY . . 
WORKDIR "/src/Devv.Gateway.Api"
RUN dotnet build "Devv.Gateway.Api.csproj" -c Release -o /app/build

# Stage 2: Publish Stage
FROM build AS publish
RUN dotnet publish "Devv.Gateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Prepare Environment with Standard ASP.NET Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Create directories for config, certs, and logs
RUN mkdir -p /etc/app-config /etc/app/certs /var/log/app /var/www/app/static /var/lib/app/data

# Copy the appsettings.json to the /etc/app-config directory
COPY Devv.Gateway.Api/appsettings.json /etc/app-config/appsettings.json

# Copy the published app from the publish stage
COPY --from=publish /app/publish .

# Stage 4: Use Chiseled Runtime Image for Final Output
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra AS final

# Set environment variables for paths
ENV CONFIG_PATH=/etc/app-config
ENV CERT_PATH=/etc/app/certs
ENV LOG_PATH=/var/log/app
ENV STATIC_CONTENT_PATH=/var/www/app/static
ENV DATA_PATH=/var/lib/app/data

# Install ICU package for full globalization support
#RUN apt-get update && apt-get install -y libicu-dev

# Copy necessary directories and files from the base stage
COPY --from=base /etc/app-config /etc/app-config
COPY --from=base /etc/app/certs /etc/app/certs
COPY --from=base /var/log/app /var/log/app
COPY --from=base /var/www/app/static /var/www/app/static
COPY --from=base /var/lib/app/data /var/lib/app/data

# Copy the application files from the base stage
COPY --from=base /app /app

# Ensure ports match your application configuration
EXPOSE 8080
EXPOSE 8081

# Set working directory
WORKDIR /app

ENTRYPOINT ["dotnet", "Devv.Gateway.Api.dll"]