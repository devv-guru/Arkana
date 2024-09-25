using Devv.Gateway.Data.Enums;

namespace Devv.Gateway.Data.Entities;

public class CertificateConfig : EntityBase
{
    public CertificateSourceType SourceType { get; set; }

    // For Local storage
    public string LocalPath { get; set; }

    // For Key Vault
    public string KeyVaultName { get; set; }
    public string KeyVaultCertificateName { get; set; }
    public Uri KeyVaultUri { get; set; }

    // For AWS
    public string AwsCertificateName { get; set; }
    public string AwsRegion { get; set; }

    // Foreign key to RouteConfig
    public Guid HostId { get; set; }
    public Host Host { get; set; }
}