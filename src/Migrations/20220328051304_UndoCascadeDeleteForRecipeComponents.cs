using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class UndoCascadeDeleteForRecipeComponents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeComponent_MultiPartRecipes_MultiPartRecipeId",
                table: "RecipeComponent");

            migrationBuilder.AlterColumn<Guid>(
                name: "MultiPartRecipeId",
                table: "RecipeComponent",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeComponent_MultiPartRecipes_MultiPartRecipeId",
                table: "RecipeComponent",
                column: "MultiPartRecipeId",
                principalTable: "MultiPartRecipes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeComponent_MultiPartRecipes_MultiPartRecipeId",
                table: "RecipeComponent");

            migrationBuilder.AlterColumn<Guid>(
                name: "MultiPartRecipeId",
                table: "RecipeComponent",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeComponent_MultiPartRecipes_MultiPartRecipeId",
                table: "RecipeComponent",
                column: "MultiPartRecipeId",
                principalTable: "MultiPartRecipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
