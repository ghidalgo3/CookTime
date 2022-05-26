using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class AddNavigationToComponentFromMpirs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MultiPartIngredientRequirement",
                table: "MultiPartIngredientRequirement");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MultiPartIngredientRequirement",
                table: "MultiPartIngredientRequirement",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MultiPartIngredientRequirement_RecipeComponentId",
                table: "MultiPartIngredientRequirement",
                column: "RecipeComponentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MultiPartIngredientRequirement",
                table: "MultiPartIngredientRequirement");

            migrationBuilder.DropIndex(
                name: "IX_MultiPartIngredientRequirement_RecipeComponentId",
                table: "MultiPartIngredientRequirement");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MultiPartIngredientRequirement",
                table: "MultiPartIngredientRequirement",
                columns: new[] { "RecipeComponentId", "Id" });
        }
    }
}
