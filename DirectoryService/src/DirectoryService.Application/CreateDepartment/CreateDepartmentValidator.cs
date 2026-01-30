using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.CreateDepartment;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(Constants.MIN_NAME_TEXT_LENGTH, Constants.MAX_NAME_TEXT_LENGTH)
            .WithError(Error.Validation(null,
                $"Название должно быть от {Constants.MIN_NAME_TEXT_LENGTH} до {Constants.MAX_NAME_TEXT_LENGTH} символов",
                "Name"));
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .Length(Constants.MIN_NAME_TEXT_LENGTH, Constants.MAX_NAME_TEXT_LENGTH)
            .WithError(Error.Validation(
                "create.department",
                $"Идентификатор должен быть быть от {Constants.MIN_NAME_TEXT_LENGTH} до {Constants.MAX_NAME_TEXT_LENGTH} символов",
                "Identifier"))
            .Matches("^[a-zA-Z\\-]+$").WithError(Error.Validation(
                "create.department",
                $"Только латиница и знак `-`",
                "Identifier"));
        RuleFor(x => x.LocationIds)
            .NotEmpty()
            .Must(items => items.Distinct().Count() == items.Length)
            .WithError(Error.Validation(
                "create.department",
                $"Идентификатор не должен быть пустым и содержать дубликаты",
                "Identifier"));
    }
}