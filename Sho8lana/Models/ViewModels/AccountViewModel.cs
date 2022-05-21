namespace Sho8lana.Models.ViewModels
{
    public class AccountViewModel
    {
        public AccountViewModel()
        {
            Services = new List<Service>();
            Contracts = new List<Contract>();
        }
        public Customer Customer { get; set; }
        public IEnumerable<Service> Services { get; set; }
        public IEnumerable<Contract> Contracts { get; set; }
        public int BoughtServices { get; set; }
        public int SelledServices { get; set; }

    }
}
