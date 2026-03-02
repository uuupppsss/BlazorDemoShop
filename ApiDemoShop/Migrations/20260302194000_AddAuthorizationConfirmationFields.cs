using System;
using ApiDemoShop.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    [DbContext(typeof(DemoShopDbContext))]
    [Migration("20260302194000_AddAuthorizationConfirmationFields")]
    public partial class AddAuthorizationConfirmationFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "auth_confirmation_code_hash",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "auth_confirmation_code_expires_at",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_authorization_confirmed",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "auth_confirmation_code_hash",
                table: "User");

            migrationBuilder.DropColumn(
                name: "auth_confirmation_code_expires_at",
                table: "User");

            migrationBuilder.DropColumn(
                name: "is_authorization_confirmed",
                table: "User");
        }
    }
}
