using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class addingContractRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BuyerIsDone",
                table: "Contracts",
                newName: "IsCanceled");

            migrationBuilder.AddColumn<bool>(
                name: "BuyerCanceled",
                table: "Contracts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ContractRateComment",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContractRateDone",
                table: "Contracts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ContractRateStars",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerCanceled",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractRateComment",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractRateDone",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractRateStars",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "IsCanceled",
                table: "Contracts",
                newName: "BuyerIsDone");
        }
    }
}
