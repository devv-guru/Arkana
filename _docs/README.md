# 📚 Arkana MCP Security Gateway - Documentation Directory

> **⚠️ IMPORTANT:** For the comprehensive documentation index with role-based navigation, see **[index.md](index.md)**

This directory contains all essential documents for understanding, developing, and deploying the Arkana MCP Security Gateway.

**🚀 Quick Start:** New to the project? Start with **[index.md](index.md)** for guided navigation.

---

## 📚 Documentation Overview

### Core Project Documents
- **[README.md](../README.md)** - Project overview and quick start guide
- **[PRD.md](PRD.md)** - Product Requirements Document with vision and success criteria
- **[Backlog.md](Backlog.md)** - Feature backlog and development ideas

### Implementation & Architecture  
- **[MCP_GATEWAY_IMPLEMENTATION.md](MCP_GATEWAY_IMPLEMENTATION.md)** - Detailed implementation plan and technical architecture
- **[ARCHITECTURE_DECISIONS.md](ARCHITECTURE_DECISIONS.md)** - Architecture Decision Records (ADRs) for key technical decisions

### Development Standards
- **[DEVELOPMENT_STANDARDS.md](DEVELOPMENT_STANDARDS.md)** - Coding standards, technology stack, and development guidelines
- **[MCP_SERVER_STANDARDS.md](MCP_SERVER_STANDARDS.md)** - Requirements for external MCP servers to integrate with the gateway

---

## 🎯 Quick Navigation

### For Product Managers
- Start with **[PRD.md](PRD.md)** for project vision and requirements
- Review **[Backlog.md](Backlog.md)** for feature priorities
- Check **[MCP_GATEWAY_IMPLEMENTATION.md](MCP_GATEWAY_IMPLEMENTATION.md)** for delivery timeline

### For Developers
- Read **[DEVELOPMENT_STANDARDS.md](DEVELOPMENT_STANDARDS.md)** for coding guidelines
- Review **[ARCHITECTURE_DECISIONS.md](ARCHITECTURE_DECISIONS.md)** for technical context
- Follow **[MCP_GATEWAY_IMPLEMENTATION.md](MCP_GATEWAY_IMPLEMENTATION.md)** for implementation details

### For External MCP Server Developers
- **[MCP_SERVER_STANDARDS.md](MCP_SERVER_STANDARDS.md)** contains everything needed to build compatible MCP servers

### For DevOps/Infrastructure
- **[DEVELOPMENT_STANDARDS.md](DEVELOPMENT_STANDARDS.md)** - Deployment standards section
- **[MCP_GATEWAY_IMPLEMENTATION.md](MCP_GATEWAY_IMPLEMENTATION.md)** - Phase 6 deployment details

---

## 🔄 Project Status

**Current Phase**: Foundation & Data Layer (Phase 1) - ✅ **COMPLETED**

**Next Phase**: Authentication & Authorization (Phase 2) - 🔄 **IN PROGRESS**

### Key Decisions Made
- ✅ Minimal APIs only (no FastEndpoints)
- ✅ Hybrid error handling (Result pattern + Exceptions)
- ✅ Azure Container Apps deployment
- ✅ Generic OIDC with Entra ID priority
- ✅ Blazor WASM + Tailwind CSS for UI
- ✅ Four connection types: HTTP, WebSocket, SSE, Webhook
- ✅ Mandatory prompt injection prevention

---

## 🛠 Technology Stack Summary

| Category | Technology | Version | Purpose |
|----------|------------|---------|---------|
| **Framework** | .NET | 9.0 | Core platform |
| **Proxy** | YARP | 2.3.0 | Reverse proxy engine |
| **Database** | Entity Framework Core | 9.0.7 | Multi-database ORM |
| **API** | Minimal APIs | - | Lightweight API endpoints |
| **Auth** | Generic OIDC | - | Multi-provider authentication |
| **UI** | Blazor WebAssembly | - | Admin interface |
| **Styling** | Tailwind CSS | - | Utility-first CSS |
| **Deployment** | Azure Container Apps | - | Serverless containers |
| **Development** | .NET Aspire | - | Local orchestration |

---

## 🚀 Getting Started

1. **Read the Vision**: Start with [PRD.md](PRD.md) to understand the project goals
2. **Understand Architecture**: Review [MCP_GATEWAY_IMPLEMENTATION.md](MCP_GATEWAY_IMPLEMENTATION.md)  
3. **Learn Standards**: Study [DEVELOPMENT_STANDARDS.md](DEVELOPMENT_STANDARDS.md)
4. **Set Up Environment**: Follow the main [README.md](../README.md) quick start
5. **Start Developing**: Begin with Phase 2 implementation tasks

---

## 📋 Standards Compliance

All code must comply with:
- **Coding Standards**: Defined in [DEVELOPMENT_STANDARDS.md](DEVELOPMENT_STANDARDS.md)
- **Security Requirements**: Authentication, authorization, and prompt injection prevention
- **Performance Targets**: <200ms P95 latency, >99.9% uptime
- **Testing Requirements**: Unit tests >80% coverage, integration tests for critical paths

---

## 🔒 Security First Approach

This project prioritizes security at every level:
- **Enterprise Authentication**: OIDC with role-based access control
- **Token Security**: OAuth 2.0 On-Behalf-Of (OBO) flow
- **AI Safety**: Mandatory prompt injection prevention
- **Audit Compliance**: Comprehensive logging for enterprise requirements
- **Transport Security**: HTTPS/WSS only, Azure-managed certificates

---

## 📞 Support & Contribution

- **Issues**: Create GitHub issues with appropriate labels
- **Discussions**: Use GitHub Discussions for questions
- **Contributing**: Follow the standards in [DEVELOPMENT_STANDARDS.md](DEVELOPMENT_STANDARDS.md)
- **Architecture Changes**: Document via ADRs in [ARCHITECTURE_DECISIONS.md](ARCHITECTURE_DECISIONS.md)

---

## 📝 Document Maintenance

This documentation is living and should be updated as the project evolves:

- **PRD**: Updated when requirements change
- **Implementation**: Updated as phases complete
- **Standards**: Updated when development practices evolve
- **ADRs**: New records added for significant decisions
- **Server Standards**: Versioned updates communicated to external developers

---

*Last updated: 2025-08-04 | Documentation maintained by: Core Development Team*