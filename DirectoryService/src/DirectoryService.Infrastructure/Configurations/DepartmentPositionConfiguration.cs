using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_position");
        builder.HasKey(d => d.Id).HasName("pk_department_position");
        builder.Property(d => d.Id).HasColumnName("id");

        builder.HasOne<Department>()
            .WithMany(d => d.Positions)
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.Property(dp => dp.DepartmentId).HasColumnName("department_id");
        builder.HasOne<Position>()
            .WithMany()
            .HasForeignKey(dp => dp.PositionId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.Property(dp => dp.PositionId).HasColumnName("position_id");
    }
}