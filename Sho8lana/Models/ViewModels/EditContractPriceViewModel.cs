using System.ComponentModel.DataAnnotations;

namespace Sho8lana.Models.ViewModels
{
    public class EditContractPriceViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "رجاءً ادخل السعر")]
        [
            DataType(DataType.Currency)
            ,Display(Name ="السعر")
            ,Range(5,5000
            ,ErrorMessage ="ادخل قيمة بحد أدنى 5 دولار وحد أقصى 5000 دولار")
        ]
        public float Price { get; set; }
        [Required(ErrorMessage = "رجاءً ادخل مدة التنفيذ")]
        [Display(Name = "مدة التنفيذ"), Range(1, 30, ErrorMessage = "اقل عدد مسموح 1 يوم واقصى عدد 30 يوم")]
        public int DeliveryTime { get; set; }

    }
}
