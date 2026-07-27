using DevOpsHub.Application.Auth;
using Xunit;

namespace DevOpsHub.Tests;

public sealed class ValidationCoverageTests
{
    [Theory]
    [InlineData("weak")]
    [InlineData("12345678")]
    [InlineData("onlylowercase")]
    public void Register_rejects_weak_passwords(string password)
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("Test User", "test@example.com", password));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("test@")]
    public void Register_rejects_invalid_email(string email)
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("Test User", email, "StrongPass123!"));
        Assert.False(result.IsValid);
    }
}
