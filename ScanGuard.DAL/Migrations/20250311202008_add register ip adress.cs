using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanGuard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addregisteripadress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationIpAddress",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationIpAddress",
                table: "AspNetUsers");
        }
    }
}
