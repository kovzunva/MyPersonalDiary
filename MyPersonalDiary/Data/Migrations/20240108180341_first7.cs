using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPersonalDiary.Data.Migrations
{
    /// <inheritdoc />
    public partial class first7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvitationCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvitationCode",
                table: "AspNetUsers");
        }
    }
}
