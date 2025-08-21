using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InforceTestTask.Migrations
{
    /// <inheritdoc />
    public partial class bd_url_code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "code",
                table: "ShortURLs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                table: "ShortURLs");
        }
    }
}
