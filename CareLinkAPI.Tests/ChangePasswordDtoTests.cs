using System.ComponentModel.DataAnnotations;
using CareLinkAPI.DTOs.Auth;
using Xunit;

namespace CareLinkAPI.Tests;

public class ChangePasswordDtoTests
{
    [Fact]
    public void ChangePasswordRequest_ValidData_PassesValidation()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123@",
            NewPassword = "NewPassword123@",
            ConfirmPassword = "NewPassword123@"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void ChangePasswordRequest_MissingCurrentPassword_FailsValidation()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "",
            NewPassword = "NewPassword123@"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ChangePasswordRequest.CurrentPassword)));
    }

    [Fact]
    public void ChangePasswordRequest_ShortNewPassword_FailsValidation()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123@",
            NewPassword = "123" // < 6 chars
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ChangePasswordRequest.NewPassword)));
    }
}
