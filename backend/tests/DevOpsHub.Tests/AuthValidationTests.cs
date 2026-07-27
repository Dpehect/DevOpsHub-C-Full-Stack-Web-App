using DevOpsHub.Application.Auth;

namespace DevOpsHub.Tests;

public sealed class AuthValidationTests
{
    [Fact]
    public void Register_rejects_weak_password()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("dev@example.com", "Developer", "weak"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Register_accepts_valid_request()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("dev@example.com", "Developer", "Strong123!"));
        Assert.True(result.IsValid);
    }
}
