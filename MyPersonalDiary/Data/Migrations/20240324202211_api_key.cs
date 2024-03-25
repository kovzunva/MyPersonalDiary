using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPersonalDiary.Data.Migrations
{
    /// <inheritdoc />
    public partial class api_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "AspNetUsers");
        }
    }
}
