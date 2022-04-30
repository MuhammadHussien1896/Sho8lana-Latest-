using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class addingBuyerSellerServiceOwnerIdsToContract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerId",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerId",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SericeOwnerId",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SericeOwnerId",
                table: "Contracts");
        }
    }
}
