namespace Devv.WebServer.Api.Features.Hosts;

public sealed class CertificateOptions
{
    public const string SectionName = "CertificatesOptions";
    public CertificateSettings[] Certificates { get; set; }
}

public class CertificateSettings
{
    public string HostName { get; set; }
    public string CertificateSource { get; set; }
    public string Location { get; set; }
    public string Password { get; set; }
}