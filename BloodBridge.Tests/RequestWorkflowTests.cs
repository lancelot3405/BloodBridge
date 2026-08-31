using System.ComponentModel.DataAnnotations;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;

namespace BloodBridge.Tests;

public sealed class RequestWorkflowTests
{
    [Theory]
    [InlineData("PENDING", "MATCHED")]
    [InlineData("MATCHED", "DONOR ACCEPTED")]
    [InlineData("DONOR ACCEPTED", "DONATION COMPLETED")]
    [InlineData("DONATION COMPLETED", "FULFILLED")]
    public void ValidStatusTransitionsAreAccepted(string current, string target)
    {
        var exception = Record.Exception(() => RequestWorkflowValidator.EnsureValidTransition(current, target));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("PENDING", "FULFILLED")]
    [InlineData("MATCHED", "FULFILLED")]
    [InlineData("FULFILLED", "PENDING")]
    [InlineData("UNKNOWN", "MATCHED")]
    public void IllegalStatusTransitionsThrowValidationException(string current, string target)
    {
        Assert.Throws<InvalidOperationException>(() =>
            RequestWorkflowValidator.EnsureValidTransition(current, target));
    }

    [Fact]
    public void BloodRequestCreationRequiresMandatoryFields()
    {
        var input = new CreateBloodRequestDto();
        var validationContext = new ValidationContext(input);
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(input, validationContext, errors, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateBloodRequestDto.HospitalId)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateBloodRequestDto.BloodGroup)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateBloodRequestDto.UnitsRequired)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateBloodRequestDto.Urgency)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateBloodRequestDto.RequiredDate)));
    }

    [Fact]
    public void CompleteBloodRequestIsValidWhenMandatoryFieldsArePresent()
    {
        var input = new CreateBloodRequestDto
        {
            HospitalId = 1,
            BloodGroup = "O+",
            UnitsRequired = 2,
            Urgency = "High",
            RequiredDate = DateTime.UtcNow.AddDays(1)
        };
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), errors, true);

        Assert.True(isValid);
        Assert.Empty(errors);
    }
}
