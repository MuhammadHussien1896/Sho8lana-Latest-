using System.ComponentModel.DataAnnotations;

namespace Sho8lana.Models.ViewModels
{
    public class RoleFormViewModel
    {
        [Required,StringLength(100)]
        public string Name { get; set; }
    }
}
