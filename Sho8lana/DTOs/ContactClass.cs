using System.ComponentModel.DataAnnotations;

namespace Sho8lana.Models
{
    public class contactClass
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set;}

    }
}
