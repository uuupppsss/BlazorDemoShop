using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrdersModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
               name: "DeliveryPrice",
               table: "Order",
               type: "decimal(18,2)",
               nullable: false,
               defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
               name: "TrackingLink",
               table: "Order",
               type: "nvarchar(255)",
               nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
