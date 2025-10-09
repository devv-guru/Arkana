# Gateway Configuration

This directory contains the configuration files for the API Gateway and Reverse Proxy.

## Configuration Structure

The gateway configuration is defined in a JSON file named `gateway-config.json`. The configuration consists of two main sections:

1. **Hosts**: Defines the host names and their associated certificates.
2. **ProxyRules**: Defines the routing rules for the proxy.

### Hosts Configuration

Each host entry contains:

- `Name`: A friendly name for the host.
- `HostNames`: An array of host names (domains) that this host configuration applies to.
- `Certificate`: Certificate configuration for TLS/SSL.
  - `Name`: A friendly name for the certificate.
  - `Source`: The source of the certificate. Can be one of:
    - `InMemory`: Certificate is loaded from memory.
    - `AzureKeyVault`: Certificate is loaded from Azure Key Vault.
    - `AwsSecretManager`: Certificate is loaded from AWS Secret Manager.
  - `SubjectAlternativeNames`: An array of subject alternative names for the certificate.
  - `KeyVaultUri`: (For Azure Key Vault) The URI of the Key Vault.
  - `KeyVaultCertificateName`: (For Azure Key Vault) The name of the certificate in Key Vault.
  - `KeyVaultCertificatePasswordName`: (For Azure Key Vault) The name of the certificate password in Key Vault.
  - `AwsRegion`: (For AWS Secret Manager) The AWS region.
  - `AwsCertificateName`: (For AWS Secret Manager) The name of the certificate in Secret Manager.
  - `AwsCertificatePasswordName`: (For AWS Secret Manager) The name of the certificate password in Secret Manager.

### ProxyRules Configuration

Each proxy rule entry contains:

- `Name`: A friendly name for the rule.
- `Hosts`: An array of host names that this rule applies to. These should match host names defined in the Hosts section.
- `PathPrefix`: The path prefix for this rule.
- `StripPrefix`: Whether to strip the path prefix before forwarding the request to the destination.
- `Methods`: An array of HTTP methods that this rule applies to.
- `Cluster`: The cluster configuration for this rule.
  - `Name`: A friendly name for the cluster.
  - `LoadBalancingPolicy`: The load balancing policy. Can be one of:
    - `RoundRobin`: Requests are distributed in a round-robin fashion.
    - `LeastRequests`: Requests are sent to the destination with the least active requests.
    - `FirstAlphabetical`: Requests are sent to the first destination alphabetically.
    - `Random`: Requests are sent to a random destination.
    - `PowerOfTwoChoices`: Requests are sent to the destination with the least active requests from two randomly selected destinations.
  - `HealthCheck`: Health check configuration for the cluster.
    - `Enabled`: Whether health checks are enabled.
    - `Interval`: The interval between health checks.
    - `Timeout`: The timeout for health checks.
    - `Threshold`: The number of consecutive failures before a destination is considered unhealthy.
    - `Path`: The path to use for health checks.
    - `Query`: The query string to use for health checks.
  - `HttpRequest`: HTTP request configuration for the cluster.
    - `Version`: The HTTP version to use.
    - `VersionPolicy`: The HTTP version policy.
    - `AllowResponseBuffering`: Whether to allow response buffering.
    - `ActivityTimeout`: The activity timeout.
  - `Transforms`: An array of transforms to apply to the request.
    - `RequestHeader`: The name of the request header to set.
    - `Set`: The value to set the request header to.
  - `Destinations`: An array of destinations for this cluster.
    - `Name`: A friendly name for the destination.
    - `Address`: The address of the destination.

## Example

See the `sample-gateway-config.json` file for a complete example of a gateway configuration.

## Multiple Hosts and Rules

A key feature of this configuration is the ability to define multiple hosts and rules, and to associate rules with multiple hosts. This allows for flexible routing based on the host name and path.

For example, you can define a rule that applies to multiple hosts, or multiple rules that apply to the same host but with different path prefixes.
