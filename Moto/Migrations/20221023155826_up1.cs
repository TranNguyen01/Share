using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moto.Migrations
{
    public partial class up1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Desciption",
                table: "Categories",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Desciption",
                table: "Brands",
                newName: "Description");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Categories",
                newName: "Desciption");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Brands",
                newName: "Desciption");
        }
    }
}
