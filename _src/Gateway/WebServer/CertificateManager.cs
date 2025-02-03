using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Data.Entities;
using Shared.Certificates;

namespace Gateway.WebServer;

public class CertificateManager
{
    private readonly ILogger<CertificateManager> _logger;
    private readonly HostCertificateCache _hostCertificateCache;


    public CertificateManager(HostCertificateCache hostCertificateCache,
        ILogger<CertificateManager> logger)
    {
        _logger = logger;
        _hostCertificateCache = hostCertificateCache;
    }

    public async Task LoadCertificateAsync(Certificate certificate, CancellationToken ct = default,
        string hostName = "localhost")
    {
        var cert = certificate.CertificateSource switch
        {
            CertificateSources.AzureKeyVault => await LoadCertificateFromAzureKeyVaultAsync(certificate.KeyVaultUri,
                certificate.KeyVaultCertificateName, certificate.KeyVaultCertificatePasswordName, ct),

            CertificateSources.AwsSecretManager => await LoadCertificateFromAwsSecretsManagerAsync(
                certificate.AwsRegion,
                certificate.AwsCertificateName, certificate.AwsCertificatePasswordName, ct),

            CertificateSources.InMemory =>
                GenerateSelfSignedCertificate(hostName, certificate?.SubjectAlternativeNames),

            _ => throw new ArgumentException("Unsupported certificate source.")
        };

        _logger.LogInformation("Certificate loaded for {HostName}", hostName);
        _hostCertificateCache.SetCertificate(hostName, cert);
        _logger.LogInformation("Certificate cached for {HostName}", hostName);
    }

    private async Task<X509Certificate2> LoadCertificateFromAzureKeyVaultAsync(string? vaultUrl,
        string? certificateSecretName,
        string? passwordSecretName,
        CancellationToken ct)
    {
        _logger.LogInformation("Loading certificate from Azure Key Vault for {CertificateName}", certificateSecretName);
        if (string.IsNullOrWhiteSpace(vaultUrl))
            throw new ArgumentNullException(nameof(vaultUrl));

        if (string.IsNullOrWhiteSpace(certificateSecretName))
            throw new ArgumentNullException(nameof(certificateSecretName));

        if (string.IsNullOrWhiteSpace(passwordSecretName))
            throw new ArgumentNullException(nameof(passwordSecretName));

        var client = new SecretClient(new Uri(vaultUrl), new DefaultAzureCredential());

        KeyVaultSecret certificateSecret = await client.GetSecretAsync(certificateSecretName);
        var base64Certificate = certificateSecret.Value;

        KeyVaultSecret passwordSecret = await client.GetSecretAsync(passwordSecretName);
        var certificatePassword = passwordSecret.Value;

        var pfxBytes = Convert.FromBase64String(base64Certificate);

        var cert = new X509Certificate2(pfxBytes, certificatePassword, X509KeyStorageFlags.MachineKeySet);
        _logger.LogInformation(
            "Certificate loaded from Azure Key Vault for {CertificateSecretName} with expiry {Expiry}",
            certificateSecretName, cert.NotAfter);

        return cert;
    }

    private async Task<X509Certificate2> LoadCertificateFromAwsSecretsManagerAsync(string? certificateSecretName,
        string? passwordSecretName, string? region,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(certificateSecretName))
            throw new ArgumentNullException(nameof(certificateSecretName));

        if (string.IsNullOrWhiteSpace(passwordSecretName))
            throw new ArgumentNullException(nameof(passwordSecretName));

        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentNullException(nameof(region));

        _logger.LogInformation("Loading certificate from AWS Secrets Manager for {CertificateName}",
            certificateSecretName);

        var secretsManagerClient = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));
        var base64Certificate = await GetSecretValueAsync(secretsManagerClient, certificateSecretName);
        var certificatePassword = await GetSecretValueAsync(secretsManagerClient, passwordSecretName);
        var pfxBytes = Convert.FromBase64String(base64Certificate);

        var cert = new X509Certificate2(pfxBytes, certificatePassword, X509KeyStorageFlags.MachineKeySet);
        _logger.LogInformation(
            "Certificate loaded from AWS Secrets Manager for {CertificateSecretName} with expiry {Expiry}",
            certificateSecretName, cert.NotAfter);

        return cert;
    }

    private static async Task<string> GetSecretValueAsync(AmazonSecretsManagerClient client, string secretName)
    {
        var request = new GetSecretValueRequest
        {
            SecretId = secretName
        };

        var response = await client.GetSecretValueAsync(request);
        return response.SecretString;
    }

    private X509Certificate2 GenerateSelfSignedCertificate(string hostName, string[]? subjectAlternativeNames)
    {
        _logger.LogInformation("Generating self-signed certificate for {HostName}", hostName);

        using var rsa = RSA.Create(2048);

        var certReq = new CertificateRequest(
            $"CN={hostName}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        certReq.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: true
            )
        );

        certReq.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection
                {
                    new Oid("1.3.6.1.5.5.7.3.1")
                },
                critical: true
            )
        );

        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName(hostName);
        
        if (subjectAlternativeNames is not null)
            foreach (var san in subjectAlternativeNames)
            {
                if (IPAddress.TryParse(san, out var ipAddress))
                    sanBuilder.AddIpAddress(ipAddress);
                else
                    sanBuilder.AddDnsName(san);
            }

        certReq.CertificateExtensions.Add(sanBuilder.Build());

        certReq.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: false, // Not a CA certificate
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true
            )
        );

        var certificate = certReq.CreateSelfSigned(
            DateTimeOffset.Now,
            DateTimeOffset.Now.AddYears(1) // Increase validity period to 1 year
        );

        _logger.LogInformation("Self-signed certificate generated for {HostName} with expiry {Expiry}", hostName,
            certificate.NotAfter);

        return certificate; // Return the certificate with its associated private key
    }
}