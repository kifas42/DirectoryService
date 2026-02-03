using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public sealed class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_location");
        builder.HasKey(d => d.Id).HasName("pk_department_location");
        builder.Property(d => d.Id).HasColumnName("id");

        builder.HasOne<Department>()
            .WithMany(d => d.Locations)
            .HasForeignKey(dl => dl.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.Property(dl => dl.DepartmentId).HasColumnName("department_id");
        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(dl => dl.LocationId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.Property(dl => dl.LocationId).HasColumnName("location_id");
    }
}