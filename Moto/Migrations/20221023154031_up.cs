using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moto.Migrations
{
    public partial class up : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Desciption",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Desciption",
                table: "Brands",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Desciption",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Desciption",
                table: "Brands");
        }
    }
}
