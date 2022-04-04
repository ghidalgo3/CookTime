using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class IngredientNutritionCountRegex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountRegex",
                table: "SRNutritionData",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountRegex",
                table: "SRNutritionData");
        }
    }
}
