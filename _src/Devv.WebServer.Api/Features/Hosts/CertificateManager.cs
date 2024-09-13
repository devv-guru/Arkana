using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;

namespace Devv.WebServer.Api.Features.Hosts;

public class CertificateManager
{
    private readonly MemoryCache _cache;
    private readonly ConcurrentDictionary<string, SecretClient> _secretClients;
    private readonly ConcurrentDictionary<string, CertificateClient> _certificateClients;
    private readonly DefaultAzureCredential _credential;

    public CertificateManager()
    {
        // Initialize MemoryCache with default options
        _cache = new MemoryCache(new MemoryCacheOptions());

        _credential = new DefaultAzureCredential();
        _secretClients = new ConcurrentDictionary<string, SecretClient>();
        _certificateClients = new ConcurrentDictionary<string, CertificateClient>();
    }

    public async Task AddOrUpdateCertificateAsync(CertificateSettings settings)
    {
        X509Certificate2 certificate;

        if (settings.CertificateSource == CertificateSources.KeyVault)
        {
            certificate = await LoadCertificateFromKeyVaultAsync(settings.Location);
        }
        else if (settings.CertificateSource == CertificateSources.LocalFile)
        {
            certificate = LoadCertificateFromLocalFile(settings.Location, settings.Password);
        }
        else
        {
            throw new ArgumentException("Unsupported certificate source.");
        }

        var expiration = certificate.NotAfter - DateTime.Now;
        if (expiration.TotalHours < 1)
        {
            expiration = TimeSpan.FromHours(1); // If the certificate is expiring soon, set a minimum cache time.
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            SlidingExpiration = TimeSpan.FromHours(6),
        };

        // Add or update certificate in the cache
        _cache.Set(settings.HostName, certificate, cacheEntryOptions);
    }

    // Get the certificate for a specific host
    public X509Certificate2 GetCertificate(string hostName)
    {
        _cache.TryGetValue(hostName, out X509Certificate2 certificate);
        return certificate;
    }

    // Remove a certificate for a specific host
    public bool RemoveCertificate(string hostName)
    {
        _cache.Remove(hostName);
        return true;
    }

    // Load certificate from Key Vault (either from Secret or Certificate)
    private async Task<X509Certificate2> LoadCertificateFromKeyVaultAsync(string keyVaultUrl)
    {
        var keyVaultUri = new Uri(keyVaultUrl);

        if (keyVaultUrl.Contains("secrets"))
        {
            // Retrieve the certificate stored as a secret
            var secretClient =
                _secretClients.GetOrAdd(keyVaultUri.Host, _ => new SecretClient(keyVaultUri, _credential));
            KeyVaultSecret secret = await secretClient.GetSecretAsync(new Uri(keyVaultUrl).Segments.Last());
            byte[] certBytes = Convert.FromBase64String(secret.Value);
            return new X509Certificate2(certBytes);
        }
        else
        {
            var certificateClient =
                _certificateClients.GetOrAdd(keyVaultUri.Host, _ => new CertificateClient(keyVaultUri, _credential));
            KeyVaultCertificateWithPolicy certificateWithPolicy =
                await certificateClient.GetCertificateAsync(new Uri(keyVaultUrl).Segments.Last());
            return new X509Certificate2(certificateWithPolicy.Cer);
        }
    }

    // Load certificate from local file system
    private X509Certificate2 LoadCertificateFromLocalFile(string filePath, string password)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The certificate file was not found: {filePath}");
        }

        return new X509Certificate2(filePath, password,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
    }
}