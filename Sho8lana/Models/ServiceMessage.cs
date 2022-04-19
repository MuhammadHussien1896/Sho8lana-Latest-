using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class ServiceMessage
    {
        [Key]
        public int MessageId { get; set; }

        [StringLength(200, MinimumLength = 1, ErrorMessage = "Message must be maximum 200 characters")]
        public string MessageContent { get; set; }
        public DateTime MessageDate { get; set; }

        //navigation properties
        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; }
    }
}
