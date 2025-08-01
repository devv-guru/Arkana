namespace Data.Enums;

public enum McpAuditEventType
{
    ConnectionAttempt = 0,
    ConnectionEstablished = 1,
    ConnectionClosed = 2,
    TokenRequested = 3,
    TokenProvisioningFailed = 4,
    AccessDenied = 5,
    AuthenticationFailed = 6,
    ConfigurationChanged = 7
}