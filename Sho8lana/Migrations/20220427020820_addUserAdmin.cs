using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class addUserAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [Country], [City], [Area], [Gender], [IsPremium], [RegisterationDate], [AboutMe], [ProfileImage], [NationalIdImage], [IsVerified], [Balance], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [ProfilePicture]) VALUES (N'439d6692-91b1-4b08-8483-46d2ccf23118', N'Admin', N'Admin', N'Egypt', N'1', N' May', N'male', 0, N'2022-04-27 00:00:00', NULL, NULL, NULL, 0, 0, N'admin', N'ADMIN', N'admin@test.com', N'ADMIN@TEST.COM', 0, N'AQAAAAEAACcQAAAAECSwW4fbTcy/3slsFRqncPcgTkGbXcq8pt0QY+yj3cVVl9IA710wLl4trTwdb5qWCg==', N'UNSYZGJ73ZNMMTMC7S3MFBANFA2ZRJKQ', N'35ccca26-eb59-4f2a-adc4-76524d5952e8', N'01000054855', 0, NULL, 1, 0, null)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete From  [dbo].[AspNetUsers] where Id='439d6692-91b1-4b08-8483-46d2ccf23118'");
        }
    }
}
