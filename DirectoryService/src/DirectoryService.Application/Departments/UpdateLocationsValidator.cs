using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments;

public class UpdateLocationsValidator : AbstractValidator<UpdateLocationsRequest>
{
    public UpdateLocationsValidator() =>
        RuleFor(x => x.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.RequiredField("locationIds"))
            .Must(items => items.Distinct().Count() == items.Length)
            .WithError(GeneralErrors.Duplicate("locationIds", "локаций"));
}