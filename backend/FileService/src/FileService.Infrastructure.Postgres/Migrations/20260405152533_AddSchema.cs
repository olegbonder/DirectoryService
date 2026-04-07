using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "video_processes",
                newName: "video_processes",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "video_process_steps",
                newName: "video_process_steps",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "media_assets",
                newName: "media_assets",
                newSchema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "video_processes",
                schema: "public",
                newName: "video_processes");

            migrationBuilder.RenameTable(
                name: "video_process_steps",
                schema: "public",
                newName: "video_process_steps");

            migrationBuilder.RenameTable(
                name: "media_assets",
                schema: "public",
                newName: "media_assets");
        }
    }
}
