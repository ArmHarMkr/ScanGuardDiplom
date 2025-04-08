using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanGuard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddHoneypot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoneypotLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttemptedUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttemptedPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoneypotLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoneypotLogs");
        }
    }
}
