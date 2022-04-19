using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class Media
    {
        public int MediaId { get; set; }
        public string MediaPath { get; set; }
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; }
    }
}
