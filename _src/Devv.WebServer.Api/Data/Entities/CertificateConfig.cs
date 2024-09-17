using Devv.WebServer.Api.Data.Enums;

namespace Devv.WebServer.Api.Data.Entities;

public class CertificateConfig
{
    public int Id { get; set; }

    public CertificateSourceType SourceType { get; set; } 

    // For Local storage
    public string LocalPath { get; set; }

    // For Key Vault
    public string KeyVaultName { get; set; }
    public string KeyVaultSecretName { get; set; }
    public Uri KeyVaultUri { get; set; }

    // For AWS
    public string AwsSecretName { get; set; }
    public string AwsRegion { get; set; }

    // Foreign key to RouteConfig
    public int RouteConfigId { get; set; }
    public RouteConfig RouteConfig { get; set; }
}