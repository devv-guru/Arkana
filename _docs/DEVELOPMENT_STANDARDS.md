# Arkana MCP Gateway - Development Standards

**Version:** 1.0  
**Last Updated:** 2025-08-04  
**Status:** APPROVED

> **📍 Navigation:** [Documentation Index](index.md) | [Architecture Decisions](ARCHITECTURE_DECISIONS.md) | [Implementation Plan](MCP_GATEWAY_IMPLEMENTATION.md) | [External Standards](MCP_SERVER_STANDARDS.md)

---

## 🎯 Project Vision

**Mission**: Create an enterprise-grade security gateway that provides "one secure door" for AI agents to access Model Context Protocol (MCP) tools across the enterprise.

**Core Objectives**:
- Centralized OIDC authentication with role-based access control
- Token brokerage for backend systems on behalf of users
- Comprehensive audit trail for compliance requirements
- Prompt injection prevention for AI safety
- High-performance proxy with minimal latency overhead

**Target Architecture**: Azure-first, cloud-native solution with local development support via .NET Aspire.

---

## 🛠 Technology Stack

### Core Framework
- **.NET 9.0** - Latest framework with native AOT support
- **YARP 2.3.0** - Microsoft's reverse proxy library  
- **Entity Framework Core 9.0.7** - Multi-database ORM
- **ASP.NET Core** - Web framework with Minimal APIs only

### Architecture Patterns
- **Clean Architecture** - Domain-centric layered architecture
- **CQRS (Command Query Responsibility Segregation)** - Separate read/write operations
- **Mediator Pattern** - [Mediator by martinothamar](https://github.com/martinothamar/Mediator) for request/response handling
- **Result Pattern** - Type-safe error handling for business operations
- **FluentValidation** - Declarative validation rules

### Authentication & Security
- **Generic OIDC** - Multi-provider support with Entra ID priority
- **Azure Key Vault** - Secret management
- **Azure Identity** - Managed identity authentication

### Data & Caching
- **Multi-database support**: SQL Server, PostgreSQL, MySQL, SQLite
- **Hybrid Caching** - L1/L2 cache with Redis backend
- **Entity Framework migrations** - Database versioning

### Observability
- **Serilog** - Structured logging with multiple sinks
- **OpenTelemetry** - Distributed tracing and metrics
- **Application Insights** - Azure monitoring integration

### UI & Frontend
- **Blazor WebAssembly** - Admin interface (NO Node.js/React)
- **Tailwind CSS** - Utility-first styling via standalone CLI
- **Color Schemes**: 
  - Option 1: Onyx Black (#2f2f2f) & Regal Gold (#d4af37)
  - Option 2: Velvet Indigo (#4c1d95) & Platinum Frost (#e2e8f0)

### Development & Deployment
- **.NET Aspire** - Local development orchestration
- **Azure Container Apps** - Production deployment target
- **TestContainers** - Integration testing with real dependencies

---

## 🏗️ Architecture Patterns & Implementation

### Architecture Scope Clarification

**IMPORTANT**: The sophisticated architecture patterns (Clean Architecture, CQRS, Mediator) apply to the **Admin System only**, not the YARP proxy itself.

```
┌─────────────────────────┐    ┌─────────────────────────┐
│    YARP Proxy           │    │    Admin System         │
│    (Simple & Fast)      │    │  (Clean Architecture)   │
│                         │    │                         │
│  • Route matching       │    │  • CQRS Commands        │
│  • JWT validation       │    │  • FluentValidation     │
│  • Token forwarding     │    │  • Mediator pattern     │
│  • Prompt injection     │    │  • Blazor WASM UI       │
│  • Basic middleware     │    │  • Complex business     │
└─────────────────────────┘    └─────────────────────────┘
```

### Clean Architecture Implementation

The **Admin System** follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                     │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                Application Layer                    │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │              Domain Layer                   │   │   │
│  │  │  ┌─────────────────────────────────────┐   │   │   │
│  │  │  │         Entities & Core Logic        │   │   │   │
│  │  │  └─────────────────────────────────────┘   │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

#### Layer Responsibilities

**Domain Layer** (`_src/Domain/`)
- Core business entities and value objects
- Domain services and business rules
- Interfaces for external dependencies
- **No dependencies** on other layers

**Application Layer** (`_src/Endpoints/`)
- CQRS commands and queries
- Application services and orchestration
- Validation rules (FluentValidation)
- **Depends only** on Domain layer

**Infrastructure Layer** (`_src/Data/`, `_src/Gateway/`)
- Entity Framework DbContext and repositories
- External service integrations
- Configuration and dependency injection
- **Implements** Domain interfaces

### CQRS Pattern Implementation

**Command Query Responsibility Segregation** separates read and write operations:

#### Commands (Write Operations)
```csharp
// Command Definition
public record CreateMcpServerCommand(McpSimpleConfiguration Configuration) : ICommand<Result<McpServer>>;

// Command Handler
public class CreateMcpServerCommandHandler : ICommandHandler<CreateMcpServerCommand, Result<McpServer>>
{
    private readonly IWriteOnlyContext _context;
    private readonly IValidator<McpSimpleConfiguration> _validator;

    public async ValueTask<Result<McpServer>> Handle(CreateMcpServerCommand command, CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await _validator.ValidateAsync(command.Configuration, cancellationToken);
        if (!validationResult.IsValid)
            return Result<McpServer>.Failure("Validation failed", "VALIDATION_ERROR");

        // Business logic
        var mcpServer = new McpServer(command.Configuration);
        
        // Persist changes
        _context.McpServers.Add(mcpServer);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<McpServer>.Success(mcpServer);
    }
}
```

#### Queries (Read Operations)
```csharp
// Query Definition
public record GetMcpServersQuery(string UserId, string[] Roles) : IQuery<McpServer[]>;

// Query Handler
public class GetMcpServersQueryHandler : IQueryHandler<GetMcpServersQuery, McpServer[]>
{
    private readonly IReadOnlyContext _context;

    public async ValueTask<McpServer[]> Handle(GetMcpServersQuery query, CancellationToken cancellationToken)
    {
        return await _context.McpServers
            .Where(s => s.UserAssignments.Any(ua => ua.UserId == query.UserId) ||
                       s.RoleAssignments.Any(ra => query.Roles.Contains(ra.RoleName)))
            .ToArrayAsync(cancellationToken);
    }
}
```

### Mediator Pattern Usage

Using **[Mediator by martinothamar](https://github.com/martinothamar/Mediator)** for decoupled request handling:

#### Registration
```csharp
// Program.cs or ServiceCollectionExtensions
services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});
```

#### Usage in Minimal APIs
```csharp
// Minimal API endpoint
public static async Task<IResult> CreateMcpServer(
    CreateMcpServerRequest request,
    IMediator mediator,
    CancellationToken cancellationToken)
{
    var command = new CreateMcpServerCommand(request.Configuration);
    var result = await mediator.Send(command, cancellationToken);
    
    return result.IsSuccess 
        ? Results.Created($"/api/mcp/servers/{result.Value.Id}", result.Value)
        : Results.BadRequest(new { error = result.Error, code = result.ErrorCode });
}
```

### FluentValidation Implementation

**Declarative validation rules** for all input models:

```csharp
public class McpSimpleConfigurationValidator : AbstractValidator<McpSimpleConfiguration>
{
    public McpSimpleConfigurationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Server name is required")
            .Length(3, 50).WithMessage("Server name must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Server name can only contain alphanumeric characters, hyphens, and underscores");

        RuleFor(x => x.Endpoint)
            .NotEmpty().WithMessage("Endpoint URL is required")
            .Must(BeValidUrl).WithMessage("Endpoint must be a valid HTTPS URL");

        RuleFor(x => x.Protocol)
            .IsInEnum().WithMessage("Protocol must be a valid MCP protocol type");

        When(x => x.Security.AuthType == McpAuthType.OAuth2, () =>
        {
            RuleFor(x => x.Security.OAuth2Settings.ClientId)
                .NotEmpty().WithMessage("OAuth2 Client ID is required");
            
            RuleFor(x => x.Security.OAuth2Settings.ClientSecret)
                .NotEmpty().WithMessage("OAuth2 Client Secret is required");
        });
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
               uri.Scheme == Uri.UriSchemeHttps;
    }
}
```

### Result Pattern Enhancement

**Enhanced Result pattern** for comprehensive error handling:

```csharp
// Enhanced Result with multiple error types
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }
    public ValidationError[]? ValidationErrors { get; init; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    
    public static Result<T> Failure(string error, string? errorCode = null) => 
        new() { IsSuccess = false, Error = error, ErrorCode = errorCode };
    
    public static Result<T> ValidationFailure(ValidationError[] errors) =>
        new() { IsSuccess = false, ValidationErrors = errors, ErrorCode = "VALIDATION_ERROR" };

    // Implicit conversion from T
    public static implicit operator Result<T>(T value) => Success(value);
}

public record ValidationError(string PropertyName, string ErrorMessage);
```

---

## 📋 Coding Standards

### 1. Project Structure
```
_src/
├── Gateway/           # Main application host (Minimal APIs only)
├── Domain/           # Business logic & interfaces
├── Data/             # Entity Framework & repositories
├── Endpoints/        # API endpoint definitions (DEPRECATED - moving to Minimal APIs)
├── Shared/           # Cross-cutting concerns & models
└── UI.BlazorWasm/    # Admin interface

_aspire/              # .NET Aspire orchestration
_docs/                # All project documentation
_test/                # Test projects (Unit + Integration)
```

### 2. File Conventions
```csharp
// File-scoped namespaces (required)
namespace Gateway.Configuration;

// Nullable reference types enabled globally
#nullable enable

// Implicit usings enabled
// No explicit using statements for common namespaces

// Interface naming
public interface IServiceName { }

// Service implementation
public class ServiceName : IServiceName { }

// Configuration classes
public class ServiceOptions
{
    public string PropertyName { get; set; } = string.Empty;
}
```

### 3. Error Handling Strategy

**Hybrid Approach**: Result pattern for recoverable errors, exceptions for system failures.

```csharp
// Result pattern for user-correctable scenarios
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error, string? errorCode = null) => 
        new() { IsSuccess = false, Error = error, ErrorCode = errorCode };
}

// Custom exceptions for system errors
public class McpGatewayException : Exception
{
    public string ErrorCode { get; }
    public McpGatewayException(string message, string errorCode) : base(message) 
        => ErrorCode = errorCode;
}
```

**When to use each**:
- **Result Pattern**: Validation errors, business rule violations, user input issues
- **Exceptions**: System failures, infrastructure issues, unrecoverable errors

### 4. API Design - Minimal APIs Only

**NO FastEndpoints, NO Controllers** - Use Minimal APIs exclusively:

```csharp
public static class McpServerEndpoints
{
    public static void MapMcpServerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/mcp/servers")
            .WithTags("MCP Servers")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/", GetMcpServers);
        group.MapPost("/", CreateMcpServer);
        group.MapPut("/{id:guid}", UpdateMcpServer);
        group.MapDelete("/{id:guid}", DeleteMcpServer);
    }

    private static async Task<IResult> GetMcpServers(
        IUserAuthorizationService authService,
        ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        var roles = user.GetRoles();
        var servers = await authService.GetUserMcpServers(userId, roles);
        return Results.Ok(servers);
    }
}
```

### 5. Dependency Injection Pattern

```csharp
// Extension method pattern for service registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceGroup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Service registrations
        services.Configure<ServiceOptions>(configuration.GetSection("Service"));
        services.AddScoped<IService, ServiceImplementation>();
        
        return services;
    }
}
```

---

## 🔐 Security Standards

### 1. Authentication Architecture

**Primary**: Azure Entra ID (Enterprise priority)  
**Secondary**: Generic OIDC providers  

```csharp
services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // Entra ID configuration (priority)
        options.Authority = configuration["Authentication:EntraId:Authority"];
        options.Audience = configuration["Authentication:EntraId:Audience"];
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    })
    .AddJwtBearer("Generic", options =>
    {
        // Generic OIDC provider fallback
        options.Authority = configuration["Authentication:Generic:Authority"];
        options.Audience = configuration["Authentication:Generic:Audience"];
    });
```

### 2. Prompt Injection Prevention

**MANDATORY** middleware for all MCP requests:

```csharp
public class PromptInjectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPromptSafetyService _safetyService;

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsMcpRequest(context) && HasRequestBody(context))
        {
            context.Request.EnableBuffering();
            var body = await ReadRequestBody(context.Request);
            
            var safetyResult = await _safetyService.AnalyzeRequest(body);
            
            if (!safetyResult.IsSafe)
            {
                await LogSecurityViolation(context, safetyResult);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Request blocked: potential prompt injection detected");
                return;
            }
            
            context.Request.Body.Position = 0;
        }
        
        await _next(context);
    }
}
```

---

## 🌐 Supported Connection Types

The gateway supports four MCP connection patterns:

### 1. HTTP (RESTful)
- **Pattern**: `/mcp/http/{server-name}/{**endpoint}`
- **Methods**: GET, POST, PUT, DELETE
- **Use Case**: Standard API calls, CRUD operations

### 2. WebSocket (Real-time)
- **Pattern**: `/mcp/ws/{server-name}`
- **Upgrade**: HTTP → WebSocket
- **Use Case**: Interactive tools, real-time collaboration

### 3. Server-Sent Events (Streaming)
- **Pattern**: `/mcp/sse/{server-name}`
- **Method**: GET with streaming response
- **Use Case**: Progress updates, live data feeds

### 4. Webhooks (Event-driven)
- **Pattern**: `/mcp/webhook/{server-name}/{event-type}`
- **Method**: POST
- **Use Case**: Async notifications, event callbacks

---

## 🧪 Testing Strategy

**Principle**: Comprehensive but not over-complicated.

### Unit Tests
- Focus on business logic and domain services
- Mock external dependencies
- Fast execution (< 1s per test)

```csharp
[Test]
public async Task McpConfigurationTranslator_Should_Generate_Valid_Yarp_Config()
{
    // Arrange
    var config = new McpSimpleConfiguration 
    { 
        Name = "test-server",
        Protocol = McpProtocolType.Http,
        Endpoint = "https://api.example.com"
    };
    
    // Act
    var result = await _translator.TranslateToYarpConfig(config);
    
    // Assert
    result.Should().NotBeNull();
    result.Routes.Should().ContainSingle(r => r.RouteId == "mcp-test-server");
}
```

### Integration Tests
- End-to-end scenarios with real dependencies
- Use TestContainers for databases and external services
- Test authentication flows and proxy behavior

```csharp
[Test]
public async Task Gateway_Should_Proxy_Authenticated_Request()
{
    // Arrange
    await using var container = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();
    
    await container.StartAsync();
    
    // Act & Assert - Full request flow
}
```

---

## 🎨 UI Design System

### Color Schemes (Choose One)

**Option 1: Onyx Black & Regal Gold**
```css
:root {
  --primary-50: #fffdf7;
  --primary-500: #d4af37;  /* Regal Gold */
  --primary-900: #b8860b;
  
  --neutral-50: #f8f8f8;
  --neutral-900: #2f2f2f;  /* Onyx Black */
  
  --background: #1a1a1a;
  --surface: #2f2f2f;
  --accent: #d4af37;
}
```

**Option 2: Velvet Indigo & Platinum Frost**
```css
:root {
  --primary-50: #f0f4ff;
  --primary-500: #4c1d95;  /* Velvet Indigo */
  --primary-900: #312e81;
  
  --neutral-50: #f8fafc;   /* Platinum Frost */
  --neutral-900: #1e293b;
  
  --background: #0f172a;
  --surface: #1e293b;
  --accent: #e2e8f0;
}
```

### Component System
```css
/* Base Components */
.dashboard-card {
  @apply bg-surface rounded-lg shadow-lg border border-neutral-700 p-6;
}

.primary-button {
  @apply bg-primary-500 hover:bg-primary-600 text-white font-medium py-2 px-4 rounded-md;
}

.data-table {
  @apply w-full bg-surface rounded-lg overflow-hidden shadow-sm;
}

.status-badge {
  @apply inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium;
}
```

---

## 🚀 Deployment Standards

### Local Development
- **.NET Aspire** for orchestration
- **Docker Compose** for dependencies (PostgreSQL, Redis)
- **Hot reload** enabled for rapid development

### Production Deployment
- **Azure Container Apps** - Primary target
- **Horizontal scaling**: 2-10 instances based on load
- **Health checks**: `/health` endpoint with dependency validation
- **Graceful shutdown**: 30-second drain period

```yaml
# Container Apps Configuration
apiVersion: app/v1
kind: ContainerApp
metadata:
  name: arkana-mcp-gateway
spec:
  template:
    containers:
    - name: gateway
      image: arkana/mcp-gateway:latest
      env:
      - name: ASPIRE_ENVIRONMENT
        value: "Production"
      resources:
        cpu: "1.0"
        memory: "2Gi"
  scale:
    minReplicas: 2
    maxReplicas: 10
    rules:
    - name: http-scaling
      http:
        metadata:
          concurrentRequests: "100"
```

---

## 📖 Documentation Standards

### Required Documentation
1. **README.md** - Project overview and quick start
2. **PRD.md** - Product requirements and vision
3. **DEVELOPMENT_STANDARDS.md** - This document
4. **MCP_SERVER_STANDARDS.md** - External server requirements
5. **API_DOCUMENTATION.md** - OpenAPI specifications
6. **DEPLOYMENT_GUIDE.md** - Production deployment instructions

### Code Documentation
- **XML comments** for public APIs
- **README** files in each major folder
- **Inline comments** for complex business logic only
- **Architecture Decision Records (ADRs)** for major decisions

---

## ✅ Definition of Done

### Feature Completion Criteria
- [ ] Code follows all established standards
- [ ] Unit tests written with >80% coverage
- [ ] Integration tests for critical paths
- [ ] Security review completed
- [ ] Documentation updated
- [ ] Performance benchmarks validated
- [ ] Accessibility compliance verified (WCAG 2.1 AA)

### Code Review Checklist
- [ ] Follows naming conventions
- [ ] Error handling implemented correctly
- [ ] Security considerations addressed
- [ ] Performance implications considered
- [ ] Documentation updated
- [ ] Tests passing
- [ ] No breaking changes without migration plan

---

## 📋 **Quick Reference Checklist**

### Before Creating New Features
- [ ] **Domain First**: Define entities and business rules in Domain layer
- [ ] **CQRS**: Separate commands (write) from queries (read)
- [ ] **Validation**: Add FluentValidation rules for all inputs
- [ ] **Result Pattern**: Use Result<T> for operations that can fail
- [ ] **Mediator**: Route requests through IMediator
- [ ] **Tests**: Write unit tests for business logic
- [ ] **Documentation**: Update relevant docs

### Code Review Checklist
- [ ] **Layer Separation**: No dependencies flowing upward
- [ ] **CQRS Implementation**: Commands modify state, queries don't
- [ ] **Validation**: All user inputs validated with FluentValidation
- [ ] **Error Handling**: Appropriate use of Result pattern vs exceptions
- [ ] **Performance**: Async/await used correctly
- [ ] **Security**: No sensitive data in logs
- [ ] **Tests**: New functionality covered by tests

### Architecture Patterns Quick Reference
| Pattern | When to Use | Example |
|---------|-------------|---------|
| **Command** | Write operations, state changes | `CreateMcpServerCommand` |
| **Query** | Read operations, data retrieval | `GetMcpServersQuery` |
| **Result<T>** | Operations that can fail gracefully | User input validation |
| **Exception** | System failures, infrastructure issues | Database connection failure |
| **FluentValidation** | Input validation rules | API request validation |
| **Mediator** | Decoupled request handling | All API endpoints |

---

*This document is living and should be updated as the project evolves. All team members are responsible for maintaining these standards.*