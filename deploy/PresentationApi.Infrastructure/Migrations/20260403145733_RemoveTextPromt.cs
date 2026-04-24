using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresentationApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTextPromt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextPrompt",
                table: "Files");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextPrompt",
                table: "Files",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
