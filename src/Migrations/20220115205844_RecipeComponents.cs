using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using babe_algorithms.Models;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class RecipeComponents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MultiPartRecipeId",
                table: "RecipeRequirement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MultiPartRecipeId",
                table: "Images",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MultiPartRecipeId",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MultiPartRecipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StaticImage = table.Column<string>(type: "text", nullable: true),
                    ServingsProduced = table.Column<double>(type: "double precision", nullable: false),
                    Cooktime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CaloriesPerServing = table.Column<double>(type: "double precision", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiPartRecipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecipeComponent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    MultiPartRecipeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeComponent_MultiPartRecipes_MultiPartRecipeId",
                        column: x => x.MultiPartRecipeId,
                        principalTable: "MultiPartRecipes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MultiPartIngredientRequirement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: true),
                    Unit = table.Column<Unit>(type: "unit", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiPartIngredientRequirement", x => new { x.RecipeComponentId, x.Id });
                    table.ForeignKey(
                        name: "FK_MultiPartIngredientRequirement_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MultiPartIngredientRequirement_RecipeComponent_RecipeCompon~",
                        column: x => x.RecipeComponentId,
                        principalTable: "RecipeComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiPartRecipeStep",
                columns: table => new
                {
                    RecipeComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiPartRecipeStep", x => new { x.RecipeComponentId, x.Id });
                    table.ForeignKey(
                        name: "FK_MultiPartRecipeStep_RecipeComponent_RecipeComponentId",
                        column: x => x.RecipeComponentId,
                        principalTable: "RecipeComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeRequirement_MultiPartRecipeId",
                table: "RecipeRequirement",
                column: "MultiPartRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_MultiPartRecipeId",
                table: "Images",
                column: "MultiPartRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MultiPartRecipeId",
                table: "Categories",
                column: "MultiPartRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiPartIngredientRequirement_IngredientId",
                table: "MultiPartIngredientRequirement",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeComponent_MultiPartRecipeId",
                table: "RecipeComponent",
                column: "MultiPartRecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_MultiPartRecipes_MultiPartRecipeId",
                table: "Categories",
                column: "MultiPartRecipeId",
                principalTable: "MultiPartRecipes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_MultiPartRecipes_MultiPartRecipeId",
                table: "Images",
                column: "MultiPartRecipeId",
                principalTable: "MultiPartRecipes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeRequirement_MultiPartRecipes_MultiPartRecipeId",
                table: "RecipeRequirement",
                column: "MultiPartRecipeId",
                principalTable: "MultiPartRecipes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_MultiPartRecipes_MultiPartRecipeId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_MultiPartRecipes_MultiPartRecipeId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeRequirement_MultiPartRecipes_MultiPartRecipeId",
                table: "RecipeRequirement");

            migrationBuilder.DropTable(
                name: "MultiPartIngredientRequirement");

            migrationBuilder.DropTable(
                name: "MultiPartRecipeStep");

            migrationBuilder.DropTable(
                name: "RecipeComponent");

            migrationBuilder.DropTable(
                name: "MultiPartRecipes");

            migrationBuilder.DropIndex(
                name: "IX_RecipeRequirement_MultiPartRecipeId",
                table: "RecipeRequirement");

            migrationBuilder.DropIndex(
                name: "IX_Images_MultiPartRecipeId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Categories_MultiPartRecipeId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "MultiPartRecipeId",
                table: "RecipeRequirement");

            migrationBuilder.DropColumn(
                name: "MultiPartRecipeId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "MultiPartRecipeId",
                table: "Categories");
        }
    }
}
