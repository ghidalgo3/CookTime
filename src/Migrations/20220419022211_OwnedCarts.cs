using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class OwnedCarts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Carts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_OwnerId",
                table: "Carts",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_OwnerId",
                table: "Carts",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_OwnerId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_OwnerId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Carts");
        }
    }
}
