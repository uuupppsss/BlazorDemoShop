using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    /// <inheritdoc />
    public partial class Delivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelfPicked",
                table: "Order");

            migrationBuilder.CreateTable(
                name: "DeliveryMethod",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),


                    Price = table.Column<decimal>(type: "decimal(19,2)", nullable: false),

                    IsActive=table.Column<bool>(type: "bit", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delivery", x => x.id);
                });

            migrationBuilder.InsertData(
               table: "DeliveryMethod",
               columns: new[] { "Name", "Price", "IsActive" },
               values: new object[,]
               {
                    {"Самовывоз", 0 , 1 },
                    { "Почта", 300, 1},
                    { "Курьер", 500, 1 }
               });

            migrationBuilder.AddColumn<string>(
                 name: "DeliveryMethodId",
                 table: "Order",
                 type: "int",
                 nullable: false,
                 defaultValue: 1);

            migrationBuilder.AddForeignKey(
              name: "FK_Order_Delivery",
              table: "Order",
              column: "DeliveryMethodId",
              principalTable: "DeliveryMethod",
              principalColumn: "id",
              onDelete: ReferentialAction.Cascade);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
               name: "IsSelfPicked",
               table: "Order",
               type: "bit",
               nullable: false,
               defaultValue: false);
        }
    }
}
