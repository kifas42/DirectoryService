using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments;

public class UpdateLocationsValidator : AbstractValidator<UpdateLocationsRequest>
{
    public UpdateLocationsValidator()
    {
        RuleFor(x => x.LocationIds)
            .NotEmpty()
            .WithError(Error.Validation(
                "update.locations",
                $"Список локаций не должен быть пустым",
                "LocationIds"))
            .Must(items => items.Distinct().Count() == items.Length)
            .WithError(Error.Validation(
                "update.locations",
                $"Список локаций не должен содержать дубликаты",
                "LocationIds"));
    }
}