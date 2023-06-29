using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class AddNavigationToComponentFromMpirsUndo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MultiPartIngredientRequirement_RecipeComponent_RecipeCompon~",
                table: "MultiPartIngredientRequirement");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecipeComponentId",
                table: "MultiPartIngredientRequirement",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_MultiPartIngredientRequirement_RecipeComponent_RecipeCompon~",
                table: "MultiPartIngredientRequirement",
                column: "RecipeComponentId",
                principalTable: "RecipeComponent",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MultiPartIngredientRequirement_RecipeComponent_RecipeCompon~",
                table: "MultiPartIngredientRequirement");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecipeComponentId",
                table: "MultiPartIngredientRequirement",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MultiPartIngredientRequirement_RecipeComponent_RecipeCompon~",
                table: "MultiPartIngredientRequirement",
                column: "RecipeComponentId",
                principalTable: "RecipeComponent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
