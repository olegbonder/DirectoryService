using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Создаем уникальный индекс на несколько полей JSONB
            migrationBuilder.Sql(@"
            CREATE UNIQUE INDEX IX_locations_address 
            ON locations (
                (address->>'Country'),
                (address->>'City'),
                (address->>'Street'),
                (address->>'HouseNumber'),
	            COALESCE(address->>'FlatNumber', '')
            );
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IX_locations_address;");
        }
    }
}
