using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babe_algorithms.Migrations
{
    /// <inheritdoc />
    public partial class Trigrams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE EXTENSION IF NOT EXISTS pg_trgm;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP EXTENSION IF EXISTS pg_trgm;
            ");
        }
    }
}
