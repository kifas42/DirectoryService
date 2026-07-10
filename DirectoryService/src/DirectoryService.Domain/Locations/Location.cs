using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;
using Entity = DirectoryService.Domain.Shared.Entity;

namespace DirectoryService.Domain.Locations;

public sealed class Location : Entity
{
    public const int MIN_LOW_LENGTH = 3;
    public const int MAX_LOW_LENGTH = 120;

    private Location() { }

    private Location(LocationId id, string name, Address address, Timezone timezone)
    {
        Id = id;
        Name = name;
        IsActive = true;
        Address = address;
        Timezone = timezone;
    }

    public LocationId Id { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public Address Address { get; private set; } = null!;

    public Timezone Timezone { get; private set; } = null!;

    public static Result<Location, Error> Create(
        LocationId id,
        string name,
        Address address,
        Timezone timezone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return GeneralErrors.RequiredField("name");
        }

        if (name.Length is < MIN_LOW_LENGTH or > MAX_LOW_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid("name", MIN_LOW_LENGTH, MAX_LOW_LENGTH);
        }

        return new Location(id, name, address, timezone);
    }

    public void SetAddress(Address address)
    {
        Address = address;
        Update();
    }

    public UnitResult<Error> SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return GeneralErrors.RequiredField("name");
        }

        if (name.Length is < MIN_LOW_LENGTH or > MAX_LOW_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid("name", MIN_LOW_LENGTH, MAX_LOW_LENGTH);
        }

        Name = name;

        Update();
        return UnitResult.Success<Error>();
    }

    public void SetTimeZone(Timezone timezone)
    {
        Timezone = timezone;
        Update();
    }
}