using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddDeptIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_department_location_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "fk_department_position_id",
                table: "department_positions");

            migrationBuilder.AddForeignKey(
                name: "fk_department_locations_department_id",
                table: "department_locations",
                column: "department_id",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_department_locations_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_department_positions_department_id",
                table: "department_positions",
                column: "department_id",
                principalTable: "positions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_department_positions_position_id",
                table: "department_positions",
                column: "position_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_department_locations_department_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "fk_department_locations_location_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "fk_department_positions_department_id",
                table: "department_positions");

            migrationBuilder.DropForeignKey(
                name: "fk_department_positions_position_id",
                table: "department_positions");

            migrationBuilder.AddForeignKey(
                name: "fk_department_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_department_position_id",
                table: "department_positions",
                column: "position_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
