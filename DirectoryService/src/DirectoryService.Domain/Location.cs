using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;
using Entity = DirectoryService.Domain.Shared.Entity;

namespace DirectoryService.Domain;

public class Location : Entity
{
    public const int MIN_LOW_LENGTH = 3;
    public const int MAX_LOW_LENGTH = 120;

    private Location() { }

    private Location(string name, Address address, Timezone timezone)
    {
        Name = name;
        Address = address;
        Timezone = timezone;
    }

    public string Name { get; private set; } = string.Empty;

    public Address Address { get; private set; } = null!;

    public Timezone Timezone { get; private set; } = null!;

    public static Result<Location, Error> Create(string name, Address address, Timezone timezone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsEmpty("name");

        if (name.Length is < MIN_LOW_LENGTH or > MAX_LOW_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid("name", MIN_LOW_LENGTH, MAX_LOW_LENGTH);
        }

        return new Location(name, address, timezone);
    }
}