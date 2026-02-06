using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class test3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_department_position",
                table: "department_position");

            migrationBuilder.DropPrimaryKey(
                name: "pk_department_location",
                table: "department_location");

            migrationBuilder.AddPrimaryKey(
                name: "pk_department_position",
                table: "department_position",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_department_location",
                table: "department_location",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_department_position_department_id",
                table: "department_position",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_location_department_id",
                table: "department_location",
                column: "department_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_department_position",
                table: "department_position");

            migrationBuilder.DropIndex(
                name: "IX_department_position_department_id",
                table: "department_position");

            migrationBuilder.DropPrimaryKey(
                name: "pk_department_location",
                table: "department_location");

            migrationBuilder.DropIndex(
                name: "IX_department_location_department_id",
                table: "department_location");

            migrationBuilder.AddPrimaryKey(
                name: "pk_department_position",
                table: "department_position",
                columns: new[] { "department_id", "position_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_department_location",
                table: "department_location",
                columns: new[] { "department_id", "location_id" });
        }
    }
}
