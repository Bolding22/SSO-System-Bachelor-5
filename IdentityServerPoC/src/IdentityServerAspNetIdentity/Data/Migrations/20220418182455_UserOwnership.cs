using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Data.Migrations
{
    public partial class UserOwnership : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "UserAliases",
                newName: "DirectoryId");

            migrationBuilder.AddColumn<Guid>(
                name: "HomeDirectoryId",
                table: "AspNetUsers",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Directory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directory", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserAliases_DirectoryId",
                table: "UserAliases",
                column: "DirectoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_HomeDirectoryId",
                table: "AspNetUsers",
                column: "HomeDirectoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Directory_HomeDirectoryId",
                table: "AspNetUsers",
                column: "HomeDirectoryId",
                principalTable: "Directory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAliases_Directory_DirectoryId",
                table: "UserAliases",
                column: "DirectoryId",
                principalTable: "Directory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Directory_HomeDirectoryId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAliases_Directory_DirectoryId",
                table: "UserAliases");

            migrationBuilder.DropTable(
                name: "Directory");

            migrationBuilder.DropIndex(
                name: "IX_UserAliases_DirectoryId",
                table: "UserAliases");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_HomeDirectoryId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HomeDirectoryId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "DirectoryId",
                table: "UserAliases",
                newName: "OrganizationId");
        }
    }
}
