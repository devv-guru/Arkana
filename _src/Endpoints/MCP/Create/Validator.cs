using FluentValidation;

namespace Endpoints.MCP.Create;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Server name is required")
            .MaximumLength(100)
            .WithMessage("Server name cannot exceed 100 characters");

        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithMessage("Endpoint is required")
            .Must(BeValidUrl)
            .WithMessage("Endpoint must be a valid HTTP or HTTPS URL");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private static bool BeValidUrl(string endpoint)
    {
        return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}