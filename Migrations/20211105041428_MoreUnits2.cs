using Microsoft.EntityFrameworkCore.Migrations;

namespace babe_algorithms.Migrations
{
    public partial class MoreUnits2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'pint'", true);
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'quart'", true);
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'gallon'", true);
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'liter'", true);
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'milligram'", true);
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'gram'", true);
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'kilogram'", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
