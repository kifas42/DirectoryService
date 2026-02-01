using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        builder.HasKey(p => p.Id).HasName("pk_position");

        // Entity Base
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Position
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasConversion(
                value => value.Value,
                value => new PositionId(value))
            .IsRequired();
        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
            .IsRequired();
        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(Constants.MAX_TEXT_LENGTH)
            .IsRequired(false);

        builder.HasIndex(p => p.Name)
            .HasFilter("is_active = true")
            .HasDatabaseName(IndexConstants.POSITION_ACTIVE_NAME)
            .IsUnique();
    }
}