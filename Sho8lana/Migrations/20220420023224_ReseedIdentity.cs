using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class ReseedIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Governorates;DBCC CHECKIDENT('Governorates', RESEED, 0); ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Governorates;DBCC CHECKIDENT('Governorates', RESEED, 0); ");
        }
    }
}
