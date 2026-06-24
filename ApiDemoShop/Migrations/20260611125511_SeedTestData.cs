using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Banners");

            migrationBuilder.Sql("DELETE FROM ProductImage");

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
