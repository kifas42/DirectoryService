using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Locations;

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
        RuleFor(x => x.Name).Length(Constants.MIN_NAME_TEXT_LENGTH, Constants.MAX_NAME_TEXT_LENGTH)
            .WithError(Error.Validation(null,
                $"Название должно быть от {Constants.MIN_NAME_TEXT_LENGTH} до {Constants.MAX_NAME_TEXT_LENGTH} символов",
                "Name"));

        RuleFor(x => x.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}