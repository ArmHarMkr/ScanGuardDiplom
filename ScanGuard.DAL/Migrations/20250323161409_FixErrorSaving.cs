using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanGuard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixErrorSaving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteScanEntities_AspNetUsers_ScanUserId",
                table: "WebsiteScanEntities");

            migrationBuilder.AlterColumn<string>(
                name: "ScanUserId",
                table: "WebsiteScanEntities",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_WebsiteScanEntities_AspNetUsers_ScanUserId",
                table: "WebsiteScanEntities",
                column: "ScanUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteScanEntities_AspNetUsers_ScanUserId",
                table: "WebsiteScanEntities");

            migrationBuilder.AlterColumn<string>(
                name: "ScanUserId",
                table: "WebsiteScanEntities",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WebsiteScanEntities_AspNetUsers_ScanUserId",
                table: "WebsiteScanEntities",
                column: "ScanUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
