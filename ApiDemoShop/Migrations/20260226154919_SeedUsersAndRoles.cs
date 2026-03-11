using ApiDemoShop.Services;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsersAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавление ролей
            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "id", "Title" },
                values: new object[,]
                {
                    { 1, "admin" },
                    { 2, "user" },
                    { 3, "superuser" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "id", "Username", "Password", "Email", "ContactPhone", "role_Id" },
                values: new object[,]
                {
                    {
                        1,
                        "admin",
                        HashService.HashMethod("admin123"), 
                        "admin@example.com",
                        "+1234567890",
                        1 // role_Id = admin
                    },
                    {
                        2,
                        "user",
                        HashService.HashMethod("user123"),
                        "user@example.com",
                        "+0987654321",
                        2 // role_Id = user
                    },
                    {
                        3,
                        "superuser",
                        HashService.HashMethod("super123"),
                        "super@example.com",
                        "+1122334455",
                        3 // role_Id = superuser
                    }
                });
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "id",
                keyValues: new object[] { 1, 2, 3 });

            // Удаление добавленных ролей
            migrationBuilder.DeleteData(
                table: "UserRole",
                keyColumn: "id",
                keyValues: new object[] { 1, 2, 3 });
        }
    }
}
