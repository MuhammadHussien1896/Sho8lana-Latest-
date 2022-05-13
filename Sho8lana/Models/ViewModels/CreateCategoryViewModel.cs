using System.ComponentModel.DataAnnotations;

namespace Sho8lana.Models.ViewModels
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage ="برجاء ادخال اسم القسم")]
        public string Name { get; set; }
        [Required(ErrorMessage = "برجاء ادخال وصف القسم")]
        public string Description { get; set; }
        [Required(ErrorMessage = "لا يمكن انشاء قسم بدون صورة")]
        public IFormFile CategoryImg { get; set; }
    }
}
