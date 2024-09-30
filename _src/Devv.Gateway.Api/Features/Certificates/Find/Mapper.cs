using Devv.Gateway.Data.Entities;

namespace Devv.Gateway.Api.Features.Certificates.Find;

public static class Mapper
{
    public static Response FromEntity(Certificate entity)
    {
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