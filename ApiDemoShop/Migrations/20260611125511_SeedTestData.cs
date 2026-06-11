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

            migrationBuilder.InsertData(
              table: "Banners",
              columns: new[] { "ImageUrl", "Order", "IsActive", "CreatedAt" },
              values: new object[,]
              {
                    { "https://localhost:7299/uploads/banners/99c190ef1ea94e258e664fcc0b3e9e11.png", 1,true,DateTime.Now},
                    { "https://localhost:7299/uploads/banners/0d8b38655740417697fe6a02a3efbf74.png", 2,true,DateTime.Now},

              });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Banners");
        }
    }
}
