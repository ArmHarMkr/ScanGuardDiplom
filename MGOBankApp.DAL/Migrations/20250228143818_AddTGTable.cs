using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MGOBankApp.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTGTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TGConnected",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TGUserEntities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TGUserToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TGUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TGConnectedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TGUserEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TGUserEntities_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TGUserEntities_ApplicationUserId",
                table: "TGUserEntities",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TGUserEntities");

            migrationBuilder.DropColumn(
                name: "TGConnected",
                table: "AspNetUsers");
        }
    }
}
