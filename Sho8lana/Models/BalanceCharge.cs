using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class BalanceCharge
    {
        [Key]
        public string PaymentId { get; set; }
        public string StripCustId { get; set; }
        public string PaymentType { get; set; }
        public int? TotalAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }


    }
}
