using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .Length(Constants.MIN_NAME_TEXT_LENGTH, Constants.MAX_NAME_TEXT_LENGTH)
            .WithError(GeneralErrors.LenghtIsInvalid(
                name: "name",
                min: Constants.MIN_NAME_TEXT_LENGTH,
                max: Constants.MAX_NAME_TEXT_LENGTH));

        RuleFor(x => x.Identifier)
            .Length(Constants.MIN_NAME_TEXT_LENGTH, Constants.MAX_NAME_TEXT_LENGTH)
            .WithError(GeneralErrors.LenghtIsInvalid("identifier", Constants.MIN_NAME_TEXT_LENGTH,
                Constants.MAX_NAME_TEXT_LENGTH))
            .Matches("^[a-zA-Z\\-]+$").WithError(Error.Validation(
                SharedErrorCodes.Validation.InvalidFormat,
                "Допускаются только латинские буквы и дефис (-)",
                "identifier"));

        RuleFor(x => x.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.RequiredField("locationIds"))
            .Must(items => items != null && items.Distinct().Count() == items.Length)
            .WithError(GeneralErrors.Duplicate("locationIds", "локаций"));
    }
}