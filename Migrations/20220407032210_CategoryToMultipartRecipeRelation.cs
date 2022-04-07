using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class CategoryToMultipartRecipeRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_MultiPartRecipes_MultiPartRecipeId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_MultiPartRecipeId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "MultiPartRecipeId",
                table: "Categories");

            migrationBuilder.CreateTable(
                name: "CategoryMultiPartRecipe",
                columns: table => new
                {
                    CategoriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    MultiPartRecipesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryMultiPartRecipe", x => new { x.CategoriesId, x.MultiPartRecipesId });
                    table.ForeignKey(
                        name: "FK_CategoryMultiPartRecipe_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryMultiPartRecipe_MultiPartRecipes_MultiPartRecipesId",
                        column: x => x.MultiPartRecipesId,
                        principalTable: "MultiPartRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryMultiPartRecipe_MultiPartRecipesId",
                table: "CategoryMultiPartRecipe",
                column: "MultiPartRecipesId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryMultiPartRecipe");

            migrationBuilder.AddColumn<Guid>(
                name: "MultiPartRecipeId",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MultiPartRecipeId",
                table: "Categories",
                column: "MultiPartRecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_MultiPartRecipes_MultiPartRecipeId",
                table: "Categories",
                column: "MultiPartRecipeId",
                principalTable: "MultiPartRecipes",
                principalColumn: "Id");
        }
    }
}
