using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sho8lana.Migrations
{
    public partial class AddDataToTableGovernorateCity1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO [Governorates] ([governorate_name_ar], [governorate_name_en]) VALUES ( N'القاهرة', 'Cairo'),( N'الجيزة', 'Giza'),( N'الأسكندرية', 'Alexandria'),( N'الدقهلية', 'Dakahlia'),( N'البحر الأحمر', 'Red Sea'),( N'البحيرة', 'Beheira'),( N'الفيوم', 'Fayoum'),( N'الغربية', 'Gharbiya'),(N'الإسماعلية', 'Ismailia'),( N'المنوفية', 'Menofia'),( N'المنيا', 'Minya'),( N'القليوبية', 'Qaliubiya'),( N'الوادي الجديد', 'New Valley'),( N'السويس', 'Suez'),( N'اسوان', 'Aswan'),( N'اسيوط', 'Assiut'),( N'بني سويف', 'Beni Suef'),( N'بورسعيد', 'Port Said'),( N'دمياط', 'Damietta'),( N'الشرقية', 'Sharkia'),( N'جنوب سيناء', 'South Sinai'),( N'كفر الشيخ', 'Kafr Al sheikh'),( N'مطروح', 'Matrouh'),( N'الأقصر', 'Luxor'),( N'قنا', 'Qena'),( N'شمال سيناء', 'North Sinai'),( N'سوهاج', 'Sohag'); ");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Governorates");
        }
    }
}
