using System.Security.Cryptography.X509Certificates;

namespace Devv.WebServer.Api.Features.Hosts;

public interface IHostCache
{
    public X509Certificate2? GetCertificate(string? hostName);
    public void SetCertificate(string hostName, X509Certificate2 certificate);
    public void RemoveCertificate(string hostName);
}