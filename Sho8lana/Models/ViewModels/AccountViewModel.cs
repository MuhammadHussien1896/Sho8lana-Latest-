namespace Sho8lana.Models.ViewModels
{
    public class AccountViewModel
    {
        public AccountViewModel()
        {
            Services = new List<Service>();
        }
        public Customer Customer { get; set; }
        public ICollection<Service> Services { get; set; }
    }
}
