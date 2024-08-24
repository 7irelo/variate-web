using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace variate.Migrations
{
    /// <inheritdoc />
    public partial class PostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_IdentityUserId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IdentityUserId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IdentityUserId1",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "Orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IdentityUserId",
                table: "Orders",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_IdentityUserId",
                table: "Orders",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_IdentityUserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IdentityUserId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "IdentityUserId",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId1",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IdentityUserId1",
                table: "Orders",
                column: "IdentityUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_IdentityUserId1",
                table: "Orders",
                column: "IdentityUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
