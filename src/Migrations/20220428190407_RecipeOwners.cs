using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class RecipeOwners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "MultiPartRecipes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MultiPartRecipes_OwnerId",
                table: "MultiPartRecipes",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MultiPartRecipes_AspNetUsers_OwnerId",
                table: "MultiPartRecipes",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MultiPartRecipes_AspNetUsers_OwnerId",
                table: "MultiPartRecipes");

            migrationBuilder.DropIndex(
                name: "IX_MultiPartRecipes_OwnerId",
                table: "MultiPartRecipes");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "MultiPartRecipes");
        }
    }
}
