using Microsoft.EntityFrameworkCore.Migrations;

namespace babe_algorithms.Migrations
{
    public partial class StaticImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaticImage",
                table: "Recipes",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StaticImage",
                table: "Recipes");
        }
    }
}
