using ApiDemoShop.Services;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
               table: "ProductType",
               columns: new[] { "id", "Title" },
               values: new object[,]
               {
                    { 1, "посуда" },
                    { 2, "материал" },
                    { 3, "техника" }
               });

            migrationBuilder.InsertData(
                table: "Tag",
                columns: new[] { "id", "Title", "TypeId"},
                values: new object[,]
                {
                    {
                        1,
                        "весы",
                        3
                       

                    },
                    {
                        2,
                        "таймер",
                        3

                    },
                    {
                        3,
                        "дерево",
                        2

                    },
                    {
                        4,
                        "стекло",
                        2

                    },
                    {
                        5,
                        "кружка",
                        1

                    }
                });

            migrationBuilder.InsertData(
               table: "ProductTag",
               columns: new[] { "id", "ProductId", "TagId" },
               values: new object[,]
               {
                    {
                        1,
                        13,
                        1


                    },
                    {
                        2,
                        23,
                        2

                    },
                    {
                        3,
                        16,
                        3

                    },
                    {
                        4,
                        17,
                        4

                    },
                    {
                        5,
                        1,
                        5

                    }
               });



        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
               table: "ProductTag",
               keyColumn: "id",
               keyValues: new object[] { 1, 2, 3, 4 ,5 });

            migrationBuilder.DeleteData(
               table: "Tag",
               keyColumn: "id",
               keyValues: new object[] { 1, 2, 3, 4, 5 });

            migrationBuilder.DeleteData(
               table: "ProductType",
               keyColumn: "id",
               keyValues: new object[] { 1, 2, 3 });

        }
    }
}
