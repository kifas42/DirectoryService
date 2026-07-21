using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParentIdProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_departments_parent_id",
                table: "departments",
                newName: "ix_departments_parent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_departments_parent_id",
                table: "departments",
                newName: "IX_departments_parent_id");
        }
    }
}
