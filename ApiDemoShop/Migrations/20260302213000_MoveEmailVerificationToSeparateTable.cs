using System;
using ApiDemoShop.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    [DbContext(typeof(DemoShopDbContext))]
    [Migration("20260302213000_MoveEmailVerificationToSeparateTable")]
    public partial class MoveEmailVerificationToSeparateTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_email_confirmed",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE [User] SET [is_email_confirmed] = [is_authorization_confirmed]");

            migrationBuilder.DropColumn(
                name: "auth_confirmation_code_hash",
                table: "User");

            migrationBuilder.DropColumn(
                name: "auth_confirmation_code_expires_at",
                table: "User");

            migrationBuilder.DropColumn(
                name: "is_authorization_confirmed",
                table: "User");

            migrationBuilder.CreateTable(
                name: "EmailVerificationCode",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    code_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationCode", x => x.id);
                    table.ForeignKey(
                        name: "FK_EmailVerificationCode_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationCode_user_id",
                table: "EmailVerificationCode",
                column: "user_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailVerificationCode");

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

            migrationBuilder.Sql("UPDATE [User] SET [is_authorization_confirmed] = [is_email_confirmed]");

            migrationBuilder.DropColumn(
                name: "is_email_confirmed",
                table: "User");
        }
    }
}
