using Microsoft.EntityFrameworkCore.Migrations;

namespace babe_algorithms.Migrations
{
    public partial class MoreUnits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'pound'", true);
            migrationBuilder.Sql("ALTER TYPE unit ADD VALUE IF NOT EXISTS 'fluid_ounce'", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
