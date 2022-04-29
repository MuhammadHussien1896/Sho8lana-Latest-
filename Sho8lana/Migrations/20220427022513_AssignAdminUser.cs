using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class AssignAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO [dbo].[AspNetUserRoles](UserId,RoleId) SELECT '439d6692-91b1-4b08-8483-46d2ccf23118' ,Id From [dbo].[AspNetRoles] ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete From [dbo].[AspNetUserRoles] where UserId='439d6692-91b1-4b08-8483-46d2ccf23118d'");
        }
    }
}
