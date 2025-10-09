using FluentValidation;
using Gateway.Endpoints;

namespace Gateway.Validation;

public class CreateMcpServerRequestValidator : AbstractValidator<CreateMcpServerRequest>
{
    public CreateMcpServerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Name must contain only alphanumeric characters, underscores, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .Must(BeValidUrl)
            .WithMessage("Endpoint must be a valid HTTP or HTTPS URL");

        RuleFor(x => x.Protocol)
            .IsInEnum()
            .WithMessage("Protocol must be a valid MCP protocol type");

        RuleFor(x => x.AuthType)
            .IsInEnum()
            .WithMessage("AuthType must be a valid authentication type");

        RuleFor(x => x.AuthSettings)
            .Must(BeValidJson)
            .When(x => !string.IsNullOrEmpty(x.AuthSettings))
            .WithMessage("AuthSettings must be valid JSON");
    }

    private static bool BeValidUrl(string endpoint)
    {
        return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool BeValidJson(string? json)
    {
        if (string.IsNullOrEmpty(json)) return true;
        
        try
        {
            System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class UpdateMcpServerRequestValidator : AbstractValidator<UpdateMcpServerRequest>
{
    public UpdateMcpServerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Name must contain only alphanumeric characters, underscores, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .Must(BeValidUrl)
            .WithMessage("Endpoint must be a valid HTTP or HTTPS URL");

        RuleFor(x => x.Protocol)
            .IsInEnum()
            .WithMessage("Protocol must be a valid MCP protocol type");

        RuleFor(x => x.AuthType)
            .IsInEnum()
            .WithMessage("AuthType must be a valid authentication type");

        RuleFor(x => x.AuthSettings)
            .Must(BeValidJson)
            .When(x => !string.IsNullOrEmpty(x.AuthSettings))
            .WithMessage("AuthSettings must be valid JSON");
    }

    private static bool BeValidUrl(string endpoint)
    {
        return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool BeValidJson(string? json)
    {
        if (string.IsNullOrEmpty(json)) return true;
        
        try
        {
            System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}