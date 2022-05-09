using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class Complain
    {
        public Complain()
        {
            Date = DateTime.Now;
        }
        public int ComplainId { get; set; }
        public string ComplainContent { get; set; }
        public string AdminReply { get; set; }
        public bool IsSolved { get; set; }
        public bool IsReturned { get; set; }
        public DateTime Date { get; set; }

        //navigation properties
        [ForeignKey("Contract")]
        public int? ContractId { get; set; }
        public virtual Contract Contract { get; set; }
    }
}
