using Data.Entities;
using Shared.Certificates;

namespace Endpoints.Certificates.Create;

public static class Mapper
{
    public static Certificate ToEntity(Request request)
    {
        var certificate = new Certificate
        {
            Name = request.Name,
            CertificateSource = request.CertificateSource,
            IsDefault = request.IsDefault,
        };

        if (request.CertificateSource == CertificateSources.AzureKeyVault)
        {
            certificate.KeyVaultName = request.KeyVaultName;
            certificate.KeyVaultUri = request.KeyVaultUri;
            certificate.KeyVaultCertificateName = request.KeyVaultCertificateName;
            certificate.KeyVaultCertificatePasswordName = request.KeyVaultCertificatePasswordName;
        }
        else if (request.CertificateSource == CertificateSources.AwsSecretManager)
        {
            certificate.AwsRegion = request.AwsRegion;
            certificate.AwsCertificateName = request.AwsCertificateName;
            certificate.AwsCertificatePasswordName = request.AwsCertificatePasswordName;
        }
        else if (request.CertificateSource == CertificateSources.InMemory)
        {
            certificate.SubjectAlternativeNames = request.SubjectAlternativeNames;
        }

        return certificate;
    }

    public static Response FromEntity(Certificate certificate)
    {
        return new Response
        {
            Id = certificate.Id,
            Name = certificate.Name,
            CertificateSource = certificate.CertificateSource,
            IsDefault = certificate.IsDefault,
            KeyVaultName = certificate.KeyVaultName,
            KeyVaultUri = certificate.KeyVaultUri,
            KeyVaultCertificateName = certificate.KeyVaultCertificateName,
            KeyVaultCertificatePasswordName = certificate.KeyVaultCertificatePasswordName,
            AwsRegion = certificate.AwsRegion,
            AwsCertificateName = certificate.AwsCertificateName,
            AwsCertificatePasswordName = certificate.AwsCertificatePasswordName,
            SubjectAlternativeNames = certificate.SubjectAlternativeNames,
            CreatedAt = certificate.CreatedAt,
            UpdatedAt = certificate.UpdatedAt
        };
    }
}