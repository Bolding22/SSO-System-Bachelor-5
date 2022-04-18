using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServerAspNetIdentity.Data.Migrations
{
    public partial class OrganizationAddedToContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Directory_HomeDirectoryId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAliases_Directory_DirectoryId",
                table: "UserAliases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Directory",
                table: "Directory");

            migrationBuilder.RenameTable(
                name: "Directory",
                newName: "Directories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Directories",
                table: "Directories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Directories_HomeDirectoryId",
                table: "AspNetUsers",
                column: "HomeDirectoryId",
                principalTable: "Directories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAliases_Directories_DirectoryId",
                table: "UserAliases",
                column: "DirectoryId",
                principalTable: "Directories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Directories_HomeDirectoryId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAliases_Directories_DirectoryId",
                table: "UserAliases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Directories",
                table: "Directories");

            migrationBuilder.RenameTable(
                name: "Directories",
                newName: "Directory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Directory",
                table: "Directory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Directory_HomeDirectoryId",
                table: "AspNetUsers",
                column: "HomeDirectoryId",
                principalTable: "Directory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAliases_Directory_DirectoryId",
                table: "UserAliases",
                column: "DirectoryId",
                principalTable: "Directory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
