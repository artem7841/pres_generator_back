using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresentationApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBytesToFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PdfBytes",
                table: "Files",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PptxBytes",
                table: "Files",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfBytes",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "PptxBytes",
                table: "Files");
        }
    }
}
