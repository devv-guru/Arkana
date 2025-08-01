using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Caching.Hybrid;

namespace Gateway.WebServer;

public class HostCertificateCache
{
    private const string CacheKeyPrefix = "HostCert-";
    private readonly ILogger<HostCertificateCache> _logger;
    private readonly HybridCache _cache;

    public HostCertificateCache(ILogger<HostCertificateCache> logger, HybridCache cache)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<X509Certificate2?> GetCertificateAsync(string? hostName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetCertificate: {HostName}", hostName);
        try
        {
            var certificate = await _cache.GetOrCreateAsync<X509Certificate2>($"{CacheKeyPrefix}{hostName}",_ => throw new NotSupportedException(), cancellationToken);
            _logger.LogInformation("Certificate found for {HostName}", hostName);
            return certificate;
        }
        catch (Exception)
        {
            _logger.LogWarning("Certificate not found for {HostName}", hostName);
            return null;
        }
    }

    public X509Certificate2? GetCertificateSync(string? hostName)
    {
        _logger.LogInformation("GetCertificateSync: {HostName}", hostName);
        try
        {
            // For synchronous access, we use GetAwaiter().GetResult() with a timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var certificate = _cache.GetAsync<X509Certificate2>($"{CacheKeyPrefix}{hostName}", cts.Token).GetAwaiter().GetResult();
            _logger.LogInformation("Certificate found for {HostName}", hostName);
            return certificate;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Certificate not found for {HostName}", hostName);
            return null;
        }
    }

    public async Task SetCertificateAsync(string hostName, X509Certificate2 certificate, CancellationToken cancellationToken = default)
    {
        var expiration = certificate.NotAfter.AddDays(-1);
        
        _logger.LogInformation("SetCertificate: {HostName} with expiration {date}", hostName, expiration);

        var options = new HybridCacheEntryOptions
        {
            Expiration = expiration - DateTimeOffset.Now
        };

        await _cache.SetAsync($"{CacheKeyPrefix}{hostName}", certificate, options, cancellationToken);
    }

    public async Task RemoveCertificateAsync(string hostName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("RemoveCertificate: {HostName}", hostName);
        await _cache.RemoveAsync($"{CacheKeyPrefix}{hostName}", cancellationToken);
    }
}