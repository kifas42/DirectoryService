using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(l => l.Id).HasName("pk_location");

        // Entity Base
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Location
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .HasConversion(
                value => value.Value,
                value => new LocationId(value))
            .IsRequired();
        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
            .IsRequired();

        builder.ComplexProperty(l => l.Address, ab =>
        {
            ab.Property(a => a.OfficeNumber)
                .HasColumnName("office_number")
                .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
                .IsRequired();
            ab.Property(a => a.BuildingNumber)
                .HasColumnName("building_number")
                .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
                .IsRequired();
            ab.Property(a => a.Street)
                .HasColumnName("street")
                .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
                .IsRequired();
            ab.Property(a => a.City)
                .HasColumnName("city")
                .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
                .IsRequired();
            ab.Property(a => a.StateOrProvince)
                .HasColumnName("state_or_province")
                .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
                .IsRequired(false);
            ab.Property(a => a.Country)
                .HasColumnName("country")
                .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
                .IsRequired();
            ab.Property(a => a.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
                .IsRequired(false);
        });
        builder.Property(l => l.Timezone)
            .HasColumnName("timezone")
            .HasConversion(tz => tz.Value, tz => Timezone.Create(tz).Value)
            .HasMaxLength(Constants.TEXT_100)
            .IsRequired();
    }
}