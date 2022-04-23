using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class BrandedFoods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<JsonDocument>(
                name: "FoodNutrients",
                table: "SRNutritionData",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SRNutritionData",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandedNutritionDataGtinUpc",
                table: "Ingredients",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BrandedNutritionData",
                columns: table => new
                {
                    GtinUpc = table.Column<string>(type: "text", nullable: false),
                    Ingredients = table.Column<string>(type: "text", nullable: false),
                    ServingSize = table.Column<double>(type: "double precision", nullable: false),
                    ServingSizeUnit = table.Column<string>(type: "text", nullable: false),
                    LabelNutrients = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    BrandedFoodCategory = table.Column<string>(type: "text", nullable: false),
                    FdcId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    FoodNutrients = table.Column<JsonDocument>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandedNutritionData", x => x.GtinUpc);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_BrandedNutritionDataGtinUpc",
                table: "Ingredients",
                column: "BrandedNutritionDataGtinUpc");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_BrandedNutritionData_BrandedNutritionDataGtinUpc",
                table: "Ingredients",
                column: "BrandedNutritionDataGtinUpc",
                principalTable: "BrandedNutritionData",
                principalColumn: "GtinUpc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_BrandedNutritionData_BrandedNutritionDataGtinUpc",
                table: "Ingredients");

            migrationBuilder.DropTable(
                name: "BrandedNutritionData");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_BrandedNutritionDataGtinUpc",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "BrandedNutritionDataGtinUpc",
                table: "Ingredients");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "FoodNutrients",
                table: "SRNutritionData",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SRNutritionData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
