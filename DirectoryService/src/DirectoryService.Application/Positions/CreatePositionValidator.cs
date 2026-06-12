using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Positions;

public class CreatePositionValidator : AbstractValidator<CreatePositionRequest>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Name)
            .Length(Constants.MIN_NAME_TEXT_LENGTH, Constants.MAX_NAME_TEXT_LENGTH)
            .WithError(GeneralErrors.LenghtIsInvalid(
                "name",
                min: Constants.MIN_NAME_TEXT_LENGTH,
                max: Constants.MAX_NAME_TEXT_LENGTH));

        RuleFor(x => x.Description)
            .MaximumLength(Constants.MAX_TEXT_LENGTH)
            .WithError(GeneralErrors.LenghtIsInvalid(name: "description", max: Constants.MAX_TEXT_LENGTH));

        RuleFor(x => x.DepartmentIds)
            .NotEmpty()
            .WithError(GeneralErrors.RequiredField("departmentIds"))
            .Must(items => items.Distinct().Count() == items.Length)
            .WithError(GeneralErrors.Duplicate("departmentIds", "департаментов"));
    }
}