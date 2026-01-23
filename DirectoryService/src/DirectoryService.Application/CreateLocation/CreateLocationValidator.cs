using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Address)
            .MustBeValueObject(addr => Address.Create(
                addr.OfficeNumber,
                addr.BuildingNumber,
                addr.Street,
                addr.City,
                addr.StateOrProvince,
                addr.Country,
                addr.PostalCode));
        RuleFor(x => x.Name).MaximumLength(120).MinimumLength(3)
            .WithError(Error.Validation(null, "Название должно быть от 3 до 120 символов", "Name"));

        RuleFor(x => x.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}