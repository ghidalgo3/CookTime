using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class SRData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SRNutritionData",
                columns: table => new
                {
                    NdbNumber = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FdcId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    FoodNutrients = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    NutrientConversionFactors = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    FoodCategory = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    FoodPortions = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SRNutritionData", x => x.NdbNumber);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SRNutritionData");
        }
    }
}
