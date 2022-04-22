using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class adding_property_IsAccepted_into_service : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Services");
        }
    }
}
