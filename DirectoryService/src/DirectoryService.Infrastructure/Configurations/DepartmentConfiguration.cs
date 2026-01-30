using DirectoryService.Domain.Department;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Path = DirectoryService.Domain.Department.Path;

namespace DirectoryService.Infrastructure.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(d => d.Id).HasName("pk_department");

        // Entity Base
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Department
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .HasConversion(
                value => value.Value,
                value => new DepartmentId(value))
            .IsRequired();
        builder.Property(d => d.Name)
            .HasColumnName("name")
            .HasMaxLength(Constants.MAX_NAME_TEXT_LENGTH)
            .IsRequired();
        builder.Property(d => d.Identifier)
            .HasColumnName("identifier")
            .HasConversion(i => i.Value, i => Identifier.Create(i).Value)
            .HasMaxLength(Constants.TEXT_100)
            .IsRequired();
        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasColumnType("ltree")
            .HasConversion(
                p => new LTree(p.Value),
                p => Path.CreateFromStringPath(p.ToString()).Value)
            .IsRequired();
        builder.Property(d => d.Depth)
            .HasColumnName("depth")
            .IsRequired();
        builder.HasOne(d => d.Parent)
            .WithMany()
            .HasForeignKey("parent_id")
            .IsRequired(false);

        builder.HasIndex(d => d.Identifier)
            .IsUnique()
            .HasDatabaseName(IndexConstants.DEPARTMENT_IDENTIFIER);

        builder.HasIndex(d => d.Path).HasMethod("gist").HasDatabaseName(IndexConstants.DEPARTMENT_PATH);
    }
}