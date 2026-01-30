using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                CREATE UNIQUE INDEX IF NOT EXISTS {IndexConstants.ADDRESS} 
                ON locations (timezone, building_number, city, country, office_number, postal_code, state_or_province, street);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"
                DROP INDEX IF EXISTS {IndexConstants.ADDRESS};
            ");
        }
    }
}
