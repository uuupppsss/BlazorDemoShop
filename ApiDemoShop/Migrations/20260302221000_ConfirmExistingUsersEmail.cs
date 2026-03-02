using ApiDemoShop.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    [DbContext(typeof(DemoShopDbContext))]
    [Migration("20260302221000_ConfirmExistingUsersEmail")]
    public partial class ConfirmExistingUsersEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [User] SET [is_email_confirmed] = 1 WHERE [is_email_confirmed] = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [User] SET [is_email_confirmed] = 0 WHERE [is_email_confirmed] = 1");
        }
    }
}
