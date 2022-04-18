using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Data.Migrations
{
    public partial class UserOwnershipNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Directory_HomeDirectoryId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "HomeDirectoryId",
                table: "AspNetUsers",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Directory_HomeDirectoryId",
                table: "AspNetUsers",
                column: "HomeDirectoryId",
                principalTable: "Directory",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Directory_HomeDirectoryId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "HomeDirectoryId",
                table: "AspNetUsers",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Directory_HomeDirectoryId",
                table: "AspNetUsers",
                column: "HomeDirectoryId",
                principalTable: "Directory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
