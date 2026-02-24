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
            .WithError(Error.Validation(
                "create.position",
                $"Название должно быть от {Constants.MIN_NAME_TEXT_LENGTH} до {Constants.MAX_NAME_TEXT_LENGTH} символов",
                "Name"));
        RuleFor(x => x.Description)
            .MaximumLength(Constants.MAX_TEXT_LENGTH)
            .WithError(Error.Validation(
                "create.position",
                $"Описание не может превышать {Constants.MAX_TEXT_LENGTH} символов",
                "Description"));

        RuleFor(x => x.DepartmentIds)
            .NotEmpty()
            .WithError(Error.Validation(
                "create.position",
                $"Список позиций не должен быть пустым",
                "PositionIds"))
            .Must(items => items.Distinct().Count() == items.Length)
            .WithError(Error.Validation(
                "create.position",
                $"Список позиций не содержать дубликаты",
                "PositionIds"));
    }
}