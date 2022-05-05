using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class addingPendingBalanceToCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "PendingBalance",
                table: "AspNetUsers",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingBalance",
                table: "AspNetUsers");
        }
    }
}
