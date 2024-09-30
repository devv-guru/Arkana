namespace Devv.Gateway.Api.Features.Certificates.Find;

public class Response
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsDefault { get; set; }
    public string? CertificateSource { get; set; }
    public string[]? SubjectAlternativeNames { get; set; }
    public AssociateWebHostResponse[] WebHosts { get; set; }
}

public class AssociateWebHostResponse
{
    public Guid WebHostId { get; set; }
    public string? Name { get; set; }
    public string? HostName { get; set; }
}