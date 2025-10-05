using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Shared;

public record Address
{
    public string OfficeNumber { get; }
    public string BuildingNumber { get; }
    public string Street { get; }
    public string City { get; }
    public string? StateOrProvince { get; }
    public string Country { get; }
    public string? PostalCode { get; }

    private Address(
        string officeNumber,
        string buildingNumber,
        string street,
        string city,
        string? stateOrProvince,
        string country,
        string? postalCode)
    {
        OfficeNumber = officeNumber;
        BuildingNumber = buildingNumber;
        Street = street;
        City = city;
        StateOrProvince = stateOrProvince;
        PostalCode = postalCode;
        Country = country;
    }

    public static Result<Address, Error> Create(
        string officeNumber,
        string buildingNumber,
        string street,
        string city,
        string? stateOrProvince,
        string country,
        string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(officeNumber))
        {
            return GeneralErrors.ValueIsEmpty(officeNumber);
        }

        if (string.IsNullOrWhiteSpace(buildingNumber))
        {
            return GeneralErrors.ValueIsEmpty(buildingNumber);
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            return GeneralErrors.ValueIsEmpty(street);
        }

        if (street.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid(street);
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return GeneralErrors.ValueIsEmpty(city);
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            return GeneralErrors.ValueIsEmpty(country);
        }

        if (city.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid(city);
        }

        if (country.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid(country);
        }

        if (postalCode is null)
        {
            return new Address(
                officeNumber,
                buildingNumber,
                street,
                city,
                stateOrProvince,
                country,
                postalCode);
        }

        if (!_internationalPostalcodeRegex.IsMatch(postalCode.Trim()))
        {
            return Error.Validation("validation.wrong.regex", $"wrong postal code", "postalCode");
        }

        return new Address(
            officeNumber,
            buildingNumber,
            street,
            city,
            stateOrProvince,
            country,
            postalCode);
    }

    private static readonly Regex _internationalPostalcodeRegex = new Regex(
        @"^(?:(?:[A-Z]{1,2}\d{1,2}[A-Z]?\s?\d[A-Z]{2})|\d{4,5}(?:\-\d{4})?|\d{3}-\d{3}|\d{6})$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
}