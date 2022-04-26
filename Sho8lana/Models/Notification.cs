using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class Notification
    {
        public Notification()
        {
            Date = DateTime.Now;
        }
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; }

        //navigation properties
        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
