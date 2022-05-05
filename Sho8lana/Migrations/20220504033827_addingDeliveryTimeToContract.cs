using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class addingDeliveryTimeToContract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryTime",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryTime",
                table: "Contracts");
        }
    }
}
