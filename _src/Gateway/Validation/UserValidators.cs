using FluentValidation;
using Gateway.Endpoints.Users;

namespace Gateway.Validation;

public class GrantAccessRequestValidator : AbstractValidator<GrantAccessRequest>
{
    public GrantAccessRequestValidator()
    {
        RuleFor(x => x.ServerId)
            .NotEqual(Guid.Empty)
            .WithMessage("ServerId must be a valid GUID");

        RuleFor(x => x.UserEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.UserEmail))
            .WithMessage("UserEmail must be a valid email address");
    }
}

public class LogEventRequestValidator : AbstractValidator<LogEventRequest>
{
    public LogEventRequestValidator()
    {
        RuleFor(x => x.ServerId)
            .NotEqual(Guid.Empty)
            .When(x => x.ServerId.HasValue)
            .WithMessage("ServerId must be a valid GUID");

        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[A-Z_]+$")
            .WithMessage("Action must be uppercase letters and underscores only");

        RuleFor(x => x.Details)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Details))
            .WithMessage("Details must not exceed 1000 characters");

        RuleFor(x => x.IpAddress)
            .Must(BeValidIpAddress)
            .When(x => !string.IsNullOrEmpty(x.IpAddress))
            .WithMessage("IpAddress must be a valid IP address");

        RuleFor(x => x.UserAgent)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.UserAgent))
            .WithMessage("UserAgent must not exceed 500 characters");
    }

    private static bool BeValidIpAddress(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress)) return true;
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }
}