FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Devv.WebServer.Api/Devv.WebServer.Api.csproj", "Devv.WebServer.Api/"]
RUN dotnet restore "Devv.WebServer.Api/Devv.WebServer.Api.csproj"
COPY . .
WORKDIR "/src/Devv.WebServer.Api"
RUN dotnet build "Devv.WebServer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Devv.WebServer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Optionally set environment variables for Kestrel port configuration
ENV ASPNETCORE_URLS="http://*:80;https://*:443"

# Entry point
ENTRYPOINT ["dotnet", "Devv.WebServer.Api.dll"]
