# Architecture Decision Records (ADRs)

**Project**: Arkana MCP Security Gateway  
**Last Updated**: 2025-08-04

> **📍 Navigation:** [Documentation Index](index.md) | [Development Standards](DEVELOPMENT_STANDARDS.md) | [Implementation Plan](MCP_GATEWAY_IMPLEMENTATION.md) | [Product Requirements](PRD.md)

---

## ADR-001: Migrate from FastEndpoints to Minimal APIs

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team

### Context
The project currently uses FastEndpoints for API endpoints, but we need to simplify the architecture and reduce dependencies while maintaining performance and developer experience.

### Decision
Migrate all API endpoints from FastEndpoints to ASP.NET Core Minimal APIs.

### Rationale
- **Simplicity**: Minimal APIs reduce cognitive overhead and dependencies
- **Performance**: Native ASP.NET Core implementation with better performance characteristics
- **Maintainability**: Fewer abstractions to understand and maintain
- **Team Preference**: Aligns with team's preference for simpler solutions
- **Future-Proof**: Microsoft's recommended approach for lightweight APIs

### Consequences
- **Positive**:
  - Reduced NuGet dependencies
  - Better integration with ASP.NET Core ecosystem
  - Simpler debugging and profiling
  - Native OpenAPI support
- **Negative**:
  - Need to refactor existing FastEndpoints code
  - Less built-in validation (need to implement manually)
  - Loss of some FastEndpoints-specific features

### Implementation Plan
1. Create new Minimal API endpoint groups
2. Migrate validation logic to custom validators
3. Update dependency injection configuration
4. Remove FastEndpoints NuGet packages
5. Update documentation and examples

---

## ADR-002: Hybrid Error Handling Strategy

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team

### Context
We need a consistent error handling strategy that balances user experience with system reliability. Different types of errors require different handling approaches.

### Decision
Implement a hybrid approach using both Result patterns and exceptions, with clear guidelines on when to use each.

### Rationale
- **Result Pattern**: Best for user-correctable errors (validation, business rules)
- **Exceptions**: Appropriate for system errors and unrecoverable failures
- **User Experience**: Result patterns provide better error messages for UI
- **System Reliability**: Exceptions ensure proper error propagation for infrastructure issues

### Guidelines
```csharp
// Use Result<T> for:
- Input validation errors
- Business rule violations  
- Configuration issues user can fix
- External service unavailable (temporary)

// Use Exceptions for:
- Database connection failures
- Authentication system failures
- Memory/resource exhaustion
- Programming errors (null reference, etc.)
```

### Consequences
- **Positive**:
  - Clear error handling patterns
  - Better user experience for recoverable errors
  - Proper system error propagation
  - Consistent across codebase
- **Negative**:
  - Two error handling patterns to learn
  - Need clear guidelines to prevent misuse

---

## ADR-003: Azure Container Apps as Primary Deployment Target

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team

### Context
We need to choose a deployment target that provides enterprise-grade scalability, security, and operational simplicity while maintaining cost effectiveness.

### Decision
Use Azure Container Apps as the primary production deployment target, with support for local development via .NET Aspire.

### Rationale
- **Serverless Containers**: Automatic scaling with pay-per-use pricing
- **Enterprise Security**: Built-in Azure AD integration and managed identity
- **Operational Simplicity**: Minimal infrastructure management overhead
- **Developer Experience**: Seamless integration with .NET Aspire for local development
- **Compliance**: Azure compliance certifications for enterprise requirements

### Alternative Considered
- **Azure App Service**: Less flexible for container workloads
- **Azure Kubernetes Service**: Too complex for initial requirements
- **Azure Container Instances**: Limited scaling and networking capabilities

### Consequences
- **Positive**:
  - Automatic scaling based on demand
  - Built-in service discovery and load balancing
  - Integrated monitoring and logging
  - Cost-effective for variable workloads
  - Strong security posture
- **Negative**:
  - Azure-specific deployment (vendor lock-in)
  - Learning curve for team unfamiliar with Container Apps
  - Limited control over underlying infrastructure

---

## ADR-004: Generic OIDC with Azure Entra ID Priority

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team

### Context
Enterprise customers use various identity providers, but Azure Entra ID is dominant in our target market. We need flexibility while optimizing for the primary use case.

### Decision
Implement generic OIDC authentication with Azure Entra ID as the primary, optimized provider.

### Rationale
- **Enterprise Focus**: Most enterprise customers use Azure Entra ID
- **Flexibility**: Support for other OIDC providers (Okta, Auth0, etc.)
- **Future-Proof**: Can adapt to changing enterprise identity landscape
- **Optimization**: Special handling for Entra ID features (groups, conditional access)

### Implementation
```csharp
services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => 
    {
        // Optimized for Entra ID
        options.Authority = config["Authentication:EntraId:Authority"];
        // Enhanced claim handling for Entra ID
    })
    .AddJwtBearer("Generic", options =>
    {
        // Generic OIDC fallback
        options.Authority = config["Authentication:Generic:Authority"];
    });
```

### Consequences
- **Positive**:
  - Best experience for Azure customers
  - Support for multi-tenant scenarios
  - Flexibility for non-Azure customers
  - Future extensibility
- **Negative**:
  - Added complexity in authentication logic
  - Need to test multiple providers
  - Documentation overhead

---

## ADR-005: Four MCP Connection Types Support

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team

### Context
MCP servers have different communication patterns depending on their use cases. We need to support various connection types while maintaining security and performance.

### Decision
Support four distinct connection types: HTTP, WebSocket, Server-Sent Events (SSE), and Webhooks.

### Connection Types
1. **HTTP**: Stateless request/response for standard API calls
2. **WebSocket**: Real-time bidirectional communication
3. **SSE**: Server-to-client streaming for progress updates
4. **Webhooks**: Event-driven callbacks for async operations

### Routing Strategy
```
/mcp/http/{server-name}/{**endpoint}     # HTTP requests
/mcp/ws/{server-name}                    # WebSocket upgrade
/mcp/sse/{server-name}                   # Server-sent events
/mcp/webhook/{server-name}/{event-type}  # Webhook callbacks
```

### Rationale
- **Comprehensive Coverage**: Supports all common MCP communication patterns
- **Clear Separation**: Distinct routes prevent routing conflicts
- **Security**: Each type can have specific security policies
- **Performance**: Optimized handling for each connection type

### Consequences
- **Positive**:
  - Supports diverse MCP server architectures
  - Clear routing and security boundaries
  - Optimized performance per connection type
  - Future extensibility
- **Negative**:
  - Increased complexity in proxy logic
  - More test scenarios to cover
  - Documentation overhead

---

## ADR-006: Prompt Injection Prevention as Mandatory Middleware

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team, Security Team

### Context
AI safety is critical for enterprise deployments. Prompt injection attacks can compromise AI systems and expose sensitive data or cause unintended actions.

### Decision
Implement mandatory prompt injection prevention middleware for all MCP requests containing user input.

### Implementation
- **Request Analysis**: Scan all POST/PUT request bodies for injection patterns
- **Pattern Detection**: Use rule-based and ML-based detection methods
- **Risk Scoring**: Assign risk scores to potentially malicious content
- **Blocking**: Block high-risk requests with detailed audit logging
- **Bypass**: Emergency bypass mechanism for administrators

### Security Patterns to Detect
- Direct prompt overrides ("Ignore previous instructions")
- Role manipulation attempts ("You are now a...")
- System prompt extraction attempts
- Jailbreaking techniques
- Indirect prompt injections via data

### Consequences
- **Positive**:
  - Enhanced AI safety for enterprise deployments
  - Comprehensive audit trail for security incidents
  - Proactive threat prevention
  - Compliance with AI governance frameworks
- **Negative**:
  - Potential false positives blocking legitimate requests
  - Performance overhead for request analysis
  - Maintenance of detection rules and models

---

## ADR-007: Blazor WebAssembly with Tailwind CSS for Admin UI

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team

### Context
We need a modern, responsive admin interface that provides excellent user experience while maintaining consistency with our .NET technology stack.

### Decision
Use Blazor WebAssembly with Tailwind CSS (via CLI, not NPM) for the admin user interface.

### Technology Stack
- **Frontend**: Blazor WebAssembly
- **Styling**: Tailwind CSS via standalone CLI
- **State Management**: Built-in Blazor state management
- **HTTP Client**: Built-in HttpClient with authentication
- **Build Tool**: MSBuild integration with Tailwind CLI

### Color Scheme Options
1. **Onyx Black & Regal Gold**: Professional, luxurious appearance
2. **Velvet Indigo & Platinum Frost**: Modern, accessible design

### Rationale
- **Technology Alignment**: Consistent with .NET backend
- **Developer Productivity**: Single language across stack
- **Performance**: WebAssembly performance for complex UI operations
- **Styling Efficiency**: Tailwind's utility-first approach
- **No NPM Dependency**: Simpler build pipeline with Tailwind CLI

### Consequences
- **Positive**:
  - Unified technology stack
  - Strong typing across frontend/backend boundary
  - Excellent developer experience
  - Modern, responsive UI capabilities
  - Simplified build and deployment pipeline
- **Negative**:
  - Learning curve for developers unfamiliar with Blazor
  - Larger initial download size compared to traditional JS SPAs
  - Limited third-party component ecosystem compared to React/Vue

---

## ADR-008: Multi-Database Support with Entity Framework Core

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team

### Context
Enterprise customers have diverse database preferences and existing infrastructure. We need flexibility while maintaining development simplicity.

### Decision
Support multiple database providers through Entity Framework Core with a unified data access layer.

### Supported Databases
- **SQL Server**: Primary enterprise database
- **PostgreSQL**: Open-source enterprise preference
- **MySQL**: Common in mixed environments
- **SQLite**: Development and small deployments

### Implementation Strategy
- **Shared Models**: Common entity definitions across all providers
- **Provider-Specific Migrations**: Separate migration files per database
- **Configuration-Driven**: Database selection via configuration
- **Testing**: Integration tests against all supported databases

### Consequences
- **Positive**:
  - Flexibility for diverse enterprise environments
  - Easy development with SQLite
  - Future-proof database strategy
  - Consistent data access patterns
- **Negative**:
  - Maintenance overhead for multiple migration files
  - Testing complexity across providers
  - Potential provider-specific limitations

---

## ADR-009: Clean Architecture with CQRS and Mediator Pattern

**Date**: 2025-08-04  
**Status**: APPROVED  
**Decision Makers**: Core Development Team

### Context
We need a scalable, maintainable architecture that supports complex business logic, clear separation of concerns, and testability while handling both simple CRUD operations and complex workflows.

### Decision
Implement Clean Architecture with CQRS (Command Query Responsibility Segregation) using the Mediator pattern for request handling.

### Architecture Components
- **[Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)**: Domain-centric layered architecture
- **[CQRS](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)**: Separate read and write operations
- **[Mediator by martinothamar](https://github.com/martinothamar/Mediator)**: High-performance mediator implementation
- **[FluentValidation](https://fluentvalidation.net/)**: Declarative validation rules
- **Result Pattern**: Type-safe error handling with comprehensive error information

### Layer Structure
```
Domain Layer (_src/Domain/)
├── Entities & Value Objects
├── Domain Services
├── Business Rules
└── Interfaces (No external dependencies)

Application Layer (_src/Endpoints/)
├── CQRS Commands & Queries
├── Command/Query Handlers
├── Validation Rules
└── Application Services

Infrastructure Layer (_src/Data/, _src/Gateway/)
├── Entity Framework DbContext
├── Repository Implementations
├── External Service Integrations
└── Configuration & DI
```

### Rationale
- **Separation of Concerns**: Clear boundaries between layers prevent coupling
- **Testability**: Domain logic isolated from infrastructure concerns
- **Scalability**: CQRS allows different optimization strategies for reads vs writes
- **Performance**: martinothamar/Mediator provides zero-allocation request dispatching
- **Maintainability**: FluentValidation keeps validation rules close to domain models
- **Error Handling**: Result pattern provides explicit, type-safe error handling

### Implementation Examples

**Command Pattern**:
```csharp
public record CreateMcpServerCommand(McpSimpleConfiguration Configuration) : ICommand<Result<McpServer>>;

public class CreateMcpServerCommandHandler : ICommandHandler<CreateMcpServerCommand, Result<McpServer>>
{
    // Validation, business logic, persistence
}
```

**Query Pattern**:
```csharp
public record GetMcpServersQuery(string UserId, string[] Roles) : IQuery<McpServer[]>;

public class GetMcpServersQueryHandler : IQueryHandler<GetMcpServersQuery, McpServer[]>
{
    // Optimized read operations
}
```

### Consequences
- **Positive**:
  - Clear separation of concerns across layers
  - Highly testable architecture
  - Scalable read/write operations
  - Type-safe error handling
  - Excellent performance with zero-allocation mediator
  - Declarative validation rules
- **Negative**:
  - Additional complexity for simple CRUD operations
  - More files and abstractions to maintain
  - Learning curve for team members unfamiliar with patterns
  - Potential over-engineering for simple features

### Migration Strategy
1. **Domain Layer**: Define core entities and business rules first
2. **Application Layer**: Implement CQRS handlers for critical operations
3. **Validation**: Add FluentValidation rules for all input models
4. **Result Pattern**: Replace exception-based error handling incrementally
5. **Infrastructure**: Implement repository pattern over direct EF usage

---

## Decision Template

For future ADRs, use this template:

```markdown
## ADR-XXX: [Decision Title]

**Date**: YYYY-MM-DD  
**Status**: PROPOSED|APPROVED|SUPERSEDED  
**Decision Makers**: [List of decision makers]

### Context
[Describe the forces at play, including technological, political, social, and project local]

### Decision
[State the architecture decision and full justification]

### Rationale
[Describe why this decision was made]

### Consequences
- **Positive**: [List positive consequences]
- **Negative**: [List negative consequences and mitigation strategies]

### Implementation Notes
[Technical details, timelines, migration strategies]
```

---

*These ADRs represent the core architectural decisions for the Arkana MCP Gateway project. They should be referenced when making related decisions and updated when circumstances change.*