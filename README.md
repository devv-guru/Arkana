
# Web Server/Reverse Proxy with OIDC and Blazor WASM UI

##  ****** THIS PROJECT IS STIL A POC ******

This project implements a **web server and reverse proxy** using **Kestrel**, **YARP (Yet Another Reverse Proxy)**, **OIDC (OpenID Connect)**, along with a **Blazor WebAssembly UI** to manage and interact with the proxy server.

## Features

- **Kestrel-based Reverse Proxy**: Powered by Kestrel and YARP (Yet Another Reverse Proxy).
- **OIDC Authentication**: Implements OpenID Connect for authenticating users.
- **Blazor WebAssembly UI**: Provides a modern UI to configure, manage, and monitor the reverse proxy.
- **Certificate Management**: Support for loading certificates from different sources (Key Vault, local, self-signed).
- **HTTPS and HTTP support**: Handles both secure (HTTPS) and non-secure (HTTP) traffic.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/your-repo-name.git
cd your-repo-name
```

### 2. Build and Run the Application

#### Using .NET CLI

```bash
# Restore and build the application
dotnet restore
dotnet build

# Run the application
dotnet run --project Devv.WebServer.Api
```

#### Using Docker

A `Dockerfile` is included to build and run the server as a container:

```bash
# Build and run the Docker image
docker build -t my-reverse-proxy .
docker run -d -p 80:80 -p 443:443 my-reverse-proxy
```

### 3. Configuring the Reverse Proxy

The reverse proxy is powered by **YARP** and configured via the `appsettings.json` file. You can modify the routes, cluster destinations, and other YARP-specific configurations there.

Example:

```json
"ReverseProxy": {
  "Routes": {
    "route1": {
      "ClusterId": "cluster1",
      "Match": {
        "Path": "/api/{**catch-all}"
      }
    }
  },
  "Clusters": {
    "cluster1": {
      "Destinations": {
        "destination1": {
          "Address": "https://api.example.com/"
        }
      }
    }
  }
}
```

### 4. OIDC Authentication Setup

This project uses **OpenID Connect (OIDC)** to authenticate users. You'll need to configure OIDC in the `appsettings.json` file with the appropriate credentials from your OIDC provider.

Example:

```json
"Authentication": {
  "OIDC": {
    "Authority": "https://your-oidc-provider.com",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "ResponseType": "code",
    "CallbackPath": "/signin-oidc"
  }
}
```

### 5. Blazor WebAssembly UI

The Blazor WASM UI allows you to interact with and manage the reverse proxy server.

To start the Blazor WASM project:

```bash
# Navigate to the Blazor project directory
cd Devv.WebServer.UI

# Restore and build the Blazor WASM app
dotnet build

# Run the Blazor app
dotnet run
```

Once the Blazor WASM UI is running, you can navigate to `http://localhost:5000` (or another specified port) to interact with the web UI.

## Configuration Options

### HTTPS and Certificate Management

You can configure HTTPS settings in the `appsettings.json` file. This project supports loading certificates from:
- **Azure Key Vault**
- **Local file system**
- **Self-signed certificates**

You can configure certificate options under the `CertificateOptions` section:

```json
"CertificateOptions": {
  "Certificates": [
    {
      "HostName": "your-domain.com",
      "CertificateSource": "KeyVault",
      "Location": "https://<your-keyvault-url>",
      "Password": null
    },
    {
      "HostName": "localhost",
      "CertificateSource": "SelfSigned",
      "Location": "Generated",
      "Password": null
    }
  ]
}
```

### Reverse Proxy Configuration

YARP allows you to configure multiple routes and clusters. You can manage them via the `appsettings.json` file under the `ReverseProxy` section.

For more advanced configurations, refer to the [YARP documentation](https://microsoft.github.io/reverse-proxy/).

## Running Tests

The project includes unit and integration tests to ensure proper functionality. You can run the tests using the .NET CLI:

```bash
dotnet test
```

## Deployment

You can deploy this project using Docker, Azure App Service, or any other cloud provider that supports .NET applications.

### Docker Deployment

To deploy the project using Docker, build the Docker image and push it to a container registry:

```bash
docker build -t my-reverse-proxy .
docker tag my-reverse-proxy:latest your-registry/my-reverse-proxy:latest
docker push your-registry/my-reverse-proxy:latest
```

You can then deploy the container image to your preferred container hosting platform.

## Contributing

Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a new branch (`git checkout -b my-feature-branch`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push the branch (`git push origin my-feature-branch`).
5. Open a Pull Request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
