using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography.X509Certificates;

namespace Gateway.WebServer;

public class HostCertificateCache
{
    private const string CacheKeyPrefix = "HostCert-";
    private readonly ILogger<HostCertificateCache> _logger;
    private readonly IAppCache _cache;

    public HostCertificateCache(ILogger<HostCertificateCache> logger, IAppCache cache)
    {
        _cache = cache;
        _logger = logger;
    }

    public X509Certificate2? GetCertificate(string? hostName)
    {
        _logger.LogInformation("GetCertificate: {HostName}", hostName);
        if (_cache.TryGetValue($"{CacheKeyPrefix}{hostName}", out X509Certificate2 certificate))
        {
            _logger.LogInformation("Certificate found for {HostName}", hostName);
            return certificate;
        }

        _logger.LogWarning("Certificate not found for {HostName}", hostName);
        return null;
    }

    public void SetCertificate(string hostName, X509Certificate2 certificate)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = certificate.NotAfter.AddDays(-1)
        };

        _logger.LogInformation("SetCertificate: {HostName} with expiration {date}", hostName,
            options.AbsoluteExpiration);

        _cache.Add($"{CacheKeyPrefix}{hostName}", certificate, options);
    }

    public void RemoveCertificate(string hostName)
    {
        _logger.LogInformation("RemoveCertificate: {HostName}", hostName);
        _cache.Remove($"{CacheKeyPrefix}{hostName}");
    }
}