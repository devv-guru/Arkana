using System.Security.Cryptography.X509Certificates;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;

namespace Devv.WebServer.Api.Features.Hosts;

public class HostCache : IHostCache
{
    private readonly ILogger<HostCache> _logger;
    private readonly IAppCache _cache;

    public HostCache(ILogger<HostCache> logger, IAppCache cache)
    {
        _cache = cache;
        _logger = logger;
    }

    public X509Certificate2? GetCertificate(string? hostName)
    {
        _logger.LogInformation("GetCertificate: {HostName}", hostName);
        if (_cache.TryGetValue(hostName, out X509Certificate2 certificate))
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
            AbsoluteExpiration = certificate.NotAfter.AddDays(-1),
        };

        _logger.LogInformation("SetCertificate: {HostName} with expiration {date}", hostName,
            options.AbsoluteExpiration);

        _cache.Add(hostName, certificate, options);
    }

    public void RemoveCertificate(string hostName)
    {
        _logger.LogInformation("RemoveCertificate: {HostName}", hostName);
        _cache.Remove(hostName);
    }
}