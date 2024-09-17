namespace Devv.WebServer.Api.WebServer;

public sealed class CertificateOptions
{
    public const string SectionName = "CertificatesOptions";
    public CertificateSettings[]? Certificates { get; init; }
}

public class CertificateSettings
{
    public string? HostName { get; init; }
    public string? CertificateSource { get; init; }
    public string? Location { get; init; }
    public string? Password { get; init; }
}