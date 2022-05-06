using System.ComponentModel.DataAnnotations;

namespace Sho8lana.Models.ViewModels
{
    public class ContractViewModel
    {
        public ContractViewModel()
        {
            PendingContracts        = new List<Contract>();
            PendingPaymentContracts = new List<Contract>();
            ActiveContracts         = new List<Contract>();
            DoneContracts           = new List<Contract>();
            CanceledContracts       = new List<Contract>();
        }
        public string UserId { get; set; }
        public ICollection<Contract> PendingContracts { get; set; }
        public ICollection<Contract> PendingPaymentContracts { get; set; }
        public ICollection<Contract> ActiveContracts { get; set; }
        public ICollection<Contract> DoneContracts { get; set; }
        public ICollection<Contract> CanceledContracts { get; set; }
        
        //edit contract price veiw model
        public int Id { get; set; }
        [Required(ErrorMessage = "رجاءً ادخل السعر")]
        [
            DataType(DataType.Currency)
            , Display(Name = "السعر")
            , Range(5, 5000
            , ErrorMessage = "ادخل قيمة بحد أدنى 5 دولار وحد أقصى 5000 دولار")
        ]
        public float Price { get; set; }
        [Required(ErrorMessage = "رجاءً ادخل مدة التنفيذ")]
        [Display(Name = "مدة التنفيذ"), Range(1, 30, ErrorMessage = "اقل عدد مسموح 1 يوم واقصى عدد 30 يوم")]
        public int DeliveryTime { get; set; }
        [Required]
        [Display(Name = "عدد النجوم")]
        public int ContractRateStars { get; set; }
        [Required(ErrorMessage = "رجاءً ادخل تعليقك على الخدمة")]
        [Display(Name = "التعليق على الخدمة")]
        public string ContractRateComment { get; set; }
    }
}
