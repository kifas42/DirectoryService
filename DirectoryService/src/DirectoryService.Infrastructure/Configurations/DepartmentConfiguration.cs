using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Path = DirectoryService.Domain.Departments.Path;

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
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at").IsRequired(false);
        builder.Property(e => e.Version).IsRowVersion();

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
                p => Path.CreateFromStringPath(p.ToString()))
            .IsRequired();
        builder.Property(d => d.Depth)
            .HasColumnName("depth")
            .IsRequired();
        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id")
            .HasConversion(
                value => value != null ? value.Value : (Guid?)null,
                value => value.HasValue ? new DepartmentId(value.Value) : null)
            .IsRequired(false);
        builder.HasOne(d => d.Parent)
            .WithMany()
            .HasForeignKey(d => d.ParentId)
            .IsRequired(false);

        builder.HasIndex(d => d.Identifier)
            .IsUnique()
            .HasDatabaseName(IndexConstants.DEPARTMENT_IDENTIFIER);

        builder.HasIndex(d => d.ParentId)
            .HasDatabaseName(IndexConstants.DEPARTMENT_PARENT_ID);

        builder.HasIndex(d => d.Path).HasMethod("gist").HasDatabaseName(IndexConstants.DEPARTMENT_PATH);
        builder.HasIndex(d => d.Name).HasMethod("gin").HasOperators("gin_trgm_ops")
            .HasDatabaseName(IndexConstants.DEPARTMENT_NAME);
    }
}