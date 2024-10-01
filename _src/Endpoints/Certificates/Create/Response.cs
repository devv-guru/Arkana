namespace Endpoints.Certificates.Create;

public class Response
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsDefault { get; set; }
    public string? CertificateSource { get; set; }

    // For Key Vault
    public string? KeyVaultName { get; set; }
    public string? KeyVaultUri { get; set; }
    public string? KeyVaultCertificateName { get; set; }
    public string? KeyVaultCertificatePasswordName { get; set; }

    // For AWS
    public string? AwsRegion { get; set; }
    public string? AwsCertificateName { get; set; }
    public string? AwsCertificatePasswordName { get; set; }

    // For Inmemory
    public string[]? SubjectAlternativeNames { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}