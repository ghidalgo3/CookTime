using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class SRDataIngredientRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NutritionDataNdbNumber",
                table: "Ingredients",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_NutritionDataNdbNumber",
                table: "Ingredients",
                column: "NutritionDataNdbNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_SRNutritionData_NutritionDataNdbNumber",
                table: "Ingredients",
                column: "NutritionDataNdbNumber",
                principalTable: "SRNutritionData",
                principalColumn: "NdbNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_SRNutritionData_NutritionDataNdbNumber",
                table: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_NutritionDataNdbNumber",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "NutritionDataNdbNumber",
                table: "Ingredients");
        }
    }
}
