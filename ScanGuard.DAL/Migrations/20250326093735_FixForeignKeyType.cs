using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanGuard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Corporations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CorpName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkerCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdminUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corporations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Corporations_AspNetUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CreateCorpRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CorporationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreateCorpRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreateCorpRequests_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Corporations_AdminUserId",
                table: "Corporations",
                column: "AdminUserId",
                unique: true,
                filter: "[AdminUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CreateCorpRequests_CorporationId",
                table: "CreateCorpRequests",
                column: "CorporationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreateCorpRequests");

            migrationBuilder.DropTable(
                name: "Corporations");
        }
    }
}
