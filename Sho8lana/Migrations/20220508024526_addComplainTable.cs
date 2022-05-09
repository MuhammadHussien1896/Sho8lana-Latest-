using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class addComplainTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComplainId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Complains",
                columns: table => new
                {
                    ComplainId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplainContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminReply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSolved = table.Column<bool>(type: "bit", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complains", x => x.ComplainId);
                    table.ForeignKey(
                        name: "FK_Complains_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ComplainId",
                table: "Contracts",
                column: "ComplainId");

            migrationBuilder.CreateIndex(
                name: "IX_Complains_ContractId",
                table: "Complains",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Complains_ComplainId",
                table: "Contracts",
                column: "ComplainId",
                principalTable: "Complains",
                principalColumn: "ComplainId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Complains_ComplainId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "Complains");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ComplainId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ComplainId",
                table: "Contracts");
        }
    }
}
