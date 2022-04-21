using System.ComponentModel.DataAnnotations;

namespace Sho8lana.Models
{
    public class Governorate
    {
        [Key]
        public int GovernorateId { get; set; }
        [StringLength(50, MinimumLength = 3)]
        public string Governorate_name_ar { get; set; }
        [StringLength(50, MinimumLength = 3)]
        public string Governorate_name_en { get; set; }
        public virtual IEnumerable<City> cities { get; set; }
    }
}
