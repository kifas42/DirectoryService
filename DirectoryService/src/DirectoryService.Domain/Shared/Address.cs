using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

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

    public static Result<Address, string> Create(
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
            return "Office number is empty";
        }

        if (string.IsNullOrWhiteSpace(buildingNumber))
        {
            return "Building number is empty";
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            return "Street is empty";
        }

        if (street.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return "Street name lenght is invalid";
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return "City is empty";
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            return "Country is empty";
        }

        if (city.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return "City name lenght is invalid";
        }

        if (country.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return "Country name lenght is invalid";
        }

        if (postalCode is not null)
        {
            if (!_internationalPostalcodeRegex.IsMatch(postalCode.Trim()))
            {
                return "Postal code is invalid";
            }
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