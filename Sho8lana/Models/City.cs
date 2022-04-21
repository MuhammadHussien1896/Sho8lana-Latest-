using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }
        [StringLength(50, MinimumLength = 3)]
        public string city_name_ar { get; set; }
        [StringLength(50, MinimumLength = 3)]
        public string city_name_en { get; set; }
        [ForeignKey("Governorate")]
        public int GovernorateId { get; set; }
        public Governorate Governorate { get; set; }

    }
}
