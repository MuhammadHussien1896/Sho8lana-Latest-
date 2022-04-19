using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Sho8lana.Models
{
    public class CustomerRequest
    {
        public CustomerRequest()
        {
            RequestDate = DateTime.Now;
        }
        [Key]
        public int RequestId { get; set; }
        public DateTime RequestDate { get; set; }

        //navigation properties
        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; }
    }
}
