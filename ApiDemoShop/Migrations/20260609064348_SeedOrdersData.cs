using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    /// <inheritdoc />
    public partial class SeedOrdersData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"SET IDENTITY_INSERT [OrderStatus] ON;");

            migrationBuilder.Sql(@"UPDATE [Order] SET status_id = 1;");

            migrationBuilder.Sql("DELETE FROM [OrderStatus] WHERE id != 1;");

            migrationBuilder.InsertData(
               table: "OrderStatus",
               columns: new[] { "id", "Title" },
               values: new object[,]
               {
                    { 2, "В сборке" },
                    { 3, "Завершен" },
                    { 4, "Отменен" },
                    { 5, "Передан в доставку" },
                    { 6, "Готов к выдаче" }
               });

            migrationBuilder.Sql(@"SET IDENTITY_INSERT [OrderStatus] OFF;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
