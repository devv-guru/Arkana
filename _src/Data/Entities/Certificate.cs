﻿namespace Data.Entities;

public class Certificate : EntityBase
{
    public string? Name { get; set; }
    public bool IsDefault { get; set; }
    public string? CertificateSource { get; set; }

    // For Azure Key Vault
    public string? KeyVaultName { get; set; }
    public string? KeyVaultUri { get; set; }
    public string? KeyVaultCertificateName { get; set; }
    public string? KeyVaultCertificatePasswordName { get; set; }

    // For Local File
    public string? FilePath { get; set; }
    public string? FilePassword { get; set; }

    // For In-Memory/Self-Signed
    public string[]? SubjectAlternativeNames { get; set; }

    // WebHosts relationship removed - Azure handles SSL termination
    // Certificate management may still be needed for other purposes
}
