# MCP Server Standards for Arkana Gateway Compatibility

**Version:** 1.0  
**Last Updated:** 2025-08-04  
**Audience:** External MCP server developers  

> **📍 Navigation:** [Documentation Index](index.md) | [Development Standards](DEVELOPMENT_STANDARDS.md) | [Architecture Decisions](ARCHITECTURE_DECISIONS.md) | [Main README](../README.md)

---

## 🎯 Overview

This document defines the requirements and standards that MCP (Model Context Protocol) servers must follow to be compatible with the Arkana MCP Security Gateway. Following these standards ensures seamless integration, optimal security, and reliable operation.

---

## 🔒 Security Requirements

### Authentication & Authorization

**MANDATORY**: All MCP servers must implement JWT Bearer token validation.

#### Token Validation Requirements
```http
Authorization: Bearer {jwt-token}
```

**Token Claims Validation**:
- `iss` (Issuer): Verify against trusted issuer list
- `aud` (Audience): Must match your server's registered audience
- `exp` (Expiration): Reject expired tokens
- `azp` (Authorized Party): Validate the client application
- `sub` (Subject): User identifier for access control

#### Sample JWT Validation (C#)
```csharp
services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-issuer.com";
        options.Audience = "your-server-audience";
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });
```

### Transport Security
- **HTTPS/WSS ONLY** in production environments
- **TLS 1.2+** minimum version
- **Valid SSL certificates** - no self-signed in production

---

## 🌐 Supported Connection Types

The Arkana Gateway supports four connection patterns. Your MCP server should implement at least one:

### 1. HTTP (RESTful) - RECOMMENDED
**Gateway Route**: `/mcp/http/{your-server-name}/{**endpoint}`

**Required Endpoints**:
```http
GET  /health                 # Health check
GET  /mcp/tools/list        # Available tools discovery  
POST /mcp/tools/call        # Tool execution
GET  /mcp/manifest          # Server capabilities manifest
```

**Request Headers**:
```http
Content-Type: application/json
Authorization: Bearer {token}
X-Correlation-ID: {uuid}
X-User-ID: {user-identifier}
User-Agent: Arkana-MCP-Gateway/1.0
```

### 2. WebSocket (Real-time)
**Gateway Route**: `/mcp/ws/{your-server-name}`

**Connection Flow**:
1. Client initiates WebSocket upgrade request
2. Server validates JWT token from query parameter or header
3. Bidirectional JSON message exchange

**Message Format**:
```json
{
  "id": "correlation-id",
  "type": "request|response|event",
  "method": "tools/list|tools/call",
  "params": { },
  "result": { },
  "error": { "code": 400, "message": "Error description" }
}
```

### 3. Server-Sent Events (Streaming)
**Gateway Route**: `/mcp/sse/{your-server-name}`

**Response Format**:
```http
Content-Type: text/event-stream
Cache-Control: no-cache
Connection: keep-alive

data: {"event": "tool-progress", "data": {"percent": 50}}

data: {"event": "tool-complete", "data": {"result": "success"}}
```

### 4. Webhooks (Event-driven)
**Gateway Route**: `/mcp/webhook/{your-server-name}/{event-type}`

**Webhook Payload**:
```json
{
  "timestamp": "2025-08-04T10:30:00Z",
  "event_type": "tool-completed",
  "correlation_id": "uuid",
  "user_id": "user-identifier",
  "data": {
    "tool_id": "calculator",
    "result": "success",
    "output": { }
  }
}
```

---

## 📄 Manifest Schema

**REQUIRED**: All servers must provide a manifest at `/mcp/manifest`

```json
{
  "name": "your-server-name",
  "version": "1.0.0",
  "description": "Brief description of server capabilities",
  "protocol": "http|websocket|sse|webhook",
  "author": {
    "name": "Your Organization",
    "email": "contact@yourorg.com"
  },
  "endpoints": {
    "health": "/health",
    "tools": "/mcp/tools",
    "manifest": "/mcp/manifest"
  },
  "authentication": {
    "type": "jwt",
    "issuer": "https://your-issuer.com",
    "audience": "your-server-audience"
  },
  "capabilities": [
    "tool-listing",
    "tool-execution",
    "streaming-responses",
    "webhook-notifications"
  ],
  "tools": [
    {
      "id": "tool-unique-id",
      "name": "Tool Display Name",
      "description": "What this tool does",
      "input_schema": {
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "type": "object",
        "properties": {
          "param1": { "type": "string", "description": "Parameter description" }
        },
        "required": ["param1"]
      },
      "output_schema": {
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "type": "object",
        "properties": {
          "result": { "type": "string" }
        }
      }
    }
  ],
  "rate_limits": {
    "requests_per_minute": 60,
    "concurrent_requests": 10
  },
  "health_check": {
    "interval_seconds": 30,
    "timeout_seconds": 5
  }
}
```

---

## 🛠 API Endpoints Specification

### Health Check Endpoint
```http
GET /health
```

**Response** (200 OK):
```json
{
  "status": "healthy|degraded|unhealthy",
  "timestamp": "2025-08-04T10:30:00Z",
  "version": "1.0.0",
  "dependencies": {
    "database": "healthy",
    "external_api": "healthy"
  },
  "metrics": {
    "uptime_seconds": 86400,
    "active_connections": 5,
    "memory_usage_mb": 256
  }
}
```

### Tools List Endpoint
```http
GET /mcp/tools/list
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "tools": [
    {
      "id": "calculator",
      "name": "Calculator",
      "description": "Performs basic arithmetic operations",
      "input_schema": { },
      "output_schema": { }
    }
  ],
  "total_count": 1,
  "server_info": {
    "name": "math-tools-server",
    "version": "1.0.0"
  }
}
```

### Tool Execution Endpoint
```http
POST /mcp/tools/call
Authorization: Bearer {token}
Content-Type: application/json
X-Correlation-ID: {uuid}
```

**Request Body**:
```json
{
  "tool_id": "calculator",
  "parameters": {
    "operation": "add",
    "a": 5,
    "b": 3
  },
  "options": {
    "timeout_seconds": 30,
    "stream_response": false
  }
}
```

**Response** (200 OK):
```json
{
  "correlation_id": "uuid",
  "tool_id": "calculator", 
  "status": "success|error|timeout",
  "result": {
    "answer": 8,
    "operation_performed": "5 + 3"
  },
  "execution_time_ms": 150,
  "timestamp": "2025-08-04T10:30:00Z"
}
```

**Error Response** (400/500):
```json
{
  "error": {
    "code": "INVALID_PARAMETERS",
    "message": "Parameter 'a' must be a number",
    "correlation_id": "uuid"
  }
}
```

---

## 🔍 Request/Response Standards

### Required Headers
```http
# Incoming (from Gateway)
Authorization: Bearer {jwt-token}
Content-Type: application/json
X-Correlation-ID: {uuid}
X-User-ID: {user-identifier}
X-Forwarded-For: {client-ip}
User-Agent: Arkana-MCP-Gateway/1.0

# Outgoing (to Gateway)  
Content-Type: application/json
X-Correlation-ID: {same-uuid}
X-Response-Time-Ms: {execution-time}
```

### Error Handling Standards
Use standard HTTP status codes:
- **200**: Success
- **400**: Bad Request (invalid parameters)
- **401**: Unauthorized (invalid/missing token)
- **403**: Forbidden (insufficient permissions)
- **404**: Not Found (tool/endpoint doesn't exist)
- **429**: Too Many Requests (rate limit exceeded)
- **500**: Internal Server Error
- **503**: Service Unavailable (temporary failure)

### Rate Limiting
Implement rate limiting per user:
- **Default**: 60 requests per minute per user
- **Burst**: Allow brief spikes up to 2x the rate limit
- **Headers**: Include rate limit info in responses

```http
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1625097600
```

---

## 📊 Performance Requirements

### Response Time Targets
- **Health Check**: < 100ms
- **Tools List**: < 500ms
- **Tool Execution**: < 30s (configurable per tool)
- **WebSocket Messages**: < 200ms

### Concurrency Limits
- **HTTP Connections**: Support minimum 100 concurrent requests
- **WebSocket Connections**: Support minimum 50 concurrent connections
- **Memory Usage**: Graceful degradation when memory constrained

### Timeouts
- **Request Timeout**: 30 seconds default
- **Connection Timeout**: 10 seconds
- **Keep-Alive**: 60 seconds for HTTP connections

---

## 🔒 Security Best Practices

### Input Validation
- **Validate all parameters** against schema
- **Sanitize string inputs** to prevent injection attacks
- **Limit payload sizes** (default: 10MB max)
- **Validate file uploads** if supported

### Audit Logging
Log the following events:
```json
{
  "timestamp": "2025-08-04T10:30:00Z",
  "event_type": "tool_execution",
  "user_id": "user-123",
  "tool_id": "calculator",
  "correlation_id": "uuid",
  "status": "success|error",
  "execution_time_ms": 150,
  "client_ip": "192.168.1.100",
  "user_agent": "Arkana-MCP-Gateway/1.0"
}
```

### Secret Management
- **Never log sensitive data** (tokens, API keys, personal information)
- **Use environment variables** for configuration
- **Rotate credentials regularly**
- **Encrypt data at rest** if storing user data

---

## 🧪 Testing & Validation

### Self-Test Endpoints
Provide testing endpoints for gateway validation:

```http
GET /mcp/test/connectivity
GET /mcp/test/authentication  
POST /mcp/test/tool-execution
```

### Integration Testing
The gateway will periodically test your server:
- **Health checks** every 30 seconds
- **Authentication validation** every 5 minutes
- **Tool execution tests** every hour (if enabled)

### Monitoring Integration
Support for external monitoring:
- **Prometheus metrics** at `/metrics` (optional)
- **Health check endpoint** with detailed status
- **Structured logging** in JSON format

---

## 📋 Compliance Checklist

Before registering with the Arkana Gateway, verify:

### Security
- [ ] JWT token validation implemented
- [ ] HTTPS/WSS enforced in production
- [ ] Input validation on all endpoints
- [ ] Rate limiting implemented
- [ ] Audit logging configured
- [ ] No sensitive data in logs

### API Compliance
- [ ] All required endpoints implemented
- [ ] Manifest endpoint returns valid schema
- [ ] Standard HTTP status codes used
- [ ] Required headers handled correctly
- [ ] Error responses follow standard format

### Performance
- [ ] Health check responds in < 100ms
- [ ] Tool execution completes within timeout
- [ ] Concurrent request limits respected
- [ ] Memory usage monitoring implemented

### Documentation
- [ ] API documentation provided
- [ ] Tool schemas documented
- [ ] Error codes documented
- [ ] Rate limits specified

---

## 🚀 Getting Started

### 1. Development Setup
```bash
# Clone starter template (if available)
git clone https://github.com/arkana/mcp-server-template

# Install dependencies
dotnet restore

# Configure authentication
cp appsettings.example.json appsettings.json
# Edit authentication settings
```

### 2. Testing Your Server
```bash
# Test health endpoint
curl -X GET https://your-server.com/health

# Test with JWT token
curl -X GET https://your-server.com/mcp/tools/list \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Validate manifest
curl -X GET https://your-server.com/mcp/manifest | jq
```

### 3. Registration with Gateway
1. **Submit manifest** to gateway administrators
2. **Provide test credentials** for validation
3. **Complete security review** process
4. **Deploy to production** environment
5. **Monitor integration** status

---

## 📚 Additional Resources

- **MCP Protocol Specification**: https://spec.modelcontextprotocol.io
- **JWT Token Validation**: https://jwt.io/introduction
- **OpenAPI Schema Generator**: https://swagger.io/tools/swagger-codegen
- **JSON Schema Validator**: https://www.jsonschemavalidator.net

---

## 📞 Support

For questions about these standards or integration issues:

- **Documentation**: See project wiki
- **Issues**: Create GitHub issue with 'mcp-server' label
- **Email**: mcp-support@arkana.com
- **Slack**: #mcp-integration channel

---

*This document is versioned and maintained by the Arkana Gateway team. MCP server developers will be notified of any breaking changes with appropriate migration time.*