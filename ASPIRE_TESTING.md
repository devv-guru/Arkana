# Aspire Testing with Dummy APIs

This setup includes dummy APIs for testing the Arkana proxy configuration:

## Dummy APIs

### DummyApi1 - User Service (Port 5001)
- **GET** `/api/users` - Get all users
- **GET** `/api/users/{id}` - Get user by ID
- **POST** `/api/users` - Create user
- **GET** `/health` - Health check

### DummyApi2 - Product Service (Port 5002)
- **GET** `/api/products` - Get all products  
- **GET** `/api/products/{id}` - Get product by ID
- **POST** `/api/products` - Create product
- **GET** `/api/categories` - Get categories
- **GET** `/health` - Health check

## Aspire Configuration

The Aspire orchestrator is configured to run:
- Redis cache
- Gateway (Arkana) on HTTPS using **SQL Server** database
- MCP Graph User Server
- DummyApi1 (User Service)
- DummyApi2 (Product Service)
- SQL Server database (containerized)
- Grafana (monitoring)
- Prometheus (metrics)

### Database Configuration

The system is configured to use **SQL Server** by default (as set in `_aspire/AspireApp1.AppHost/appsettings.json`). The Gateway automatically detects the database provider and configures:

- **SQL Server**: Uses `SqlServerWriteOnlyContext` for proxy data and `GatewayDbContext` for MCP data
- **PostgreSQL**: Uses `PostgreSqlWriteOnlyContext` for proxy data and `GatewayDbContext` for MCP data  
- **MySQL/MariaDB**: Uses `MySqlWriteOnlyContext` for proxy data and `GatewayDbContext` for MCP data
- **SQLite**: Uses `SqLiteWriteOnlyContext` for proxy data and `GatewayDbContext` for MCP data (fallback)

To change the database provider, modify the `DataContextOptions:Provider` setting in the Aspire configuration.

## Running the Setup

1. Navigate to the Aspire AppHost directory:
   ```bash
   cd _aspire\AspireApp1.AppHost
   ```

2. Run the Aspire orchestrator:
   ```bash
   dotnet run
   ```

3. Open the Aspire dashboard (usually https://localhost:15888)

## Testing Proxy Configuration

The gateway can be configured to proxy requests to the dummy APIs using the configuration endpoints:

1. Use `/api/configuration/update` to load the dummy APIs configuration
2. Test proxy routing through the gateway
3. Monitor health checks and metrics

## Sample Configuration

A sample configuration for the dummy APIs is provided in:
`_src/Gateway/_config/dummy-apis-config.json`

This configuration sets up:
- Users API proxy on port 7001 → routes to localhost:5001
- Products API proxy on port 7002 → routes to localhost:5002
- Health checks enabled for both services