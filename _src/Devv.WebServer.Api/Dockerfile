# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Devv.WebServer.Api/Devv.WebServer.Api.csproj", "Devv.WebServer.Api/"]
RUN dotnet restore "Devv.WebServer.Api/Devv.WebServer.Api.csproj"

# Move this line here to optimize build caching
COPY . . 
WORKDIR "/src/Devv.WebServer.Api"
RUN dotnet build "Devv.WebServer.Api.csproj" -c Release -o /app/build

# Stage 2: Publish Stage
FROM build AS publish
RUN dotnet publish "Devv.WebServer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Use Distroless Runtime Image for more security
FROM mcr.microsoft.com/dotnet/aspnet:8.0-distroless AS final

# Create directories for config, certs, and logs
RUN mkdir -p /etc/app-config /etc/app/certs /var/log/app /var/www/app/static /var/lib/app/data

# Copy the appsettings.json to the /etc/app-config directory
COPY Devv.WebServer.Api/appsettings.json /etc/app-config/appsettings.json

# Set environment variables for paths
ENV CONFIG_PATH=/etc/app-config
ENV CERT_PATH=/etc/app/certs
ENV LOG_PATH=/var/log/app
ENV STATIC_CONTENT_PATH=/var/www/app/static
ENV DATA_PATH=/var/lib/app/data

WORKDIR /app
COPY --from=publish /app/publish .

# Ensure ports match your application configuration
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Devv.WebServer.Api.dll"]
