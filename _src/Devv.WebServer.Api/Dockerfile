# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Devv.WebServer.Api/Devv.WebServer.Api.csproj", "Devv.WebServer.Api/"]
RUN dotnet restore "Devv.WebServer.Api/Devv.WebServer.Api.csproj"
COPY . .
WORKDIR "/src/Devv.WebServer.Api"
RUN dotnet build "Devv.WebServer.Api.csproj" -c Release -o /app/build

# Stage 2: Publish Stage
FROM build AS publish
RUN dotnet publish "Devv.WebServer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Use Distroless Runtime Image for more security
FROM mcr.microsoft.com/dotnet/aspnet:8.0-distroless AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Devv.WebServer.Api.dll"]
