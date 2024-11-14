using Data.Entities.Proxy;

namespace Endpoints.Certificates.Get;

public static class Mapper
{
    public static Response? FromEntity(Certificate? entity)
    {
        if (entity == null)
            return null;
        
        return new Response
        {
            Id = entity.Id,
            Name = entity.Name,
            IsDefault = entity.IsDefault,
            CertificateSource = entity.CertificateSource,
            SubjectAlternativeNames = entity.SubjectAlternativeNames,
            WebHosts = entity.WebHosts.Select(x => new AssociateWebHostResponse
            {
                WebHostId = x.Id,
                Name = x.Name,
                HostName = x.HostName
            }).ToArray()
        };
    }
}