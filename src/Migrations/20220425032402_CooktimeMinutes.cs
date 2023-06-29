using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    public partial class CooktimeMinutes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cooktime",
                table: "MultiPartRecipes");

            migrationBuilder.AddColumn<int>(
                name: "CooktimeMinutes",
                table: "MultiPartRecipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CooktimeMinutes",
                table: "MultiPartRecipes");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Cooktime",
                table: "MultiPartRecipes",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
