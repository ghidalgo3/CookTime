using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace babe_algorithms.Migrations
{
    public partial class Images : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: true),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_RecipeId",
                table: "Images",
                column: "RecipeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
