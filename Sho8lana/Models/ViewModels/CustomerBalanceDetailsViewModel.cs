namespace Sho8lana.Models.ViewModels
{
    public class CustomerBalanceDetailsViewModel
    {
        public CustomerBalanceDetailsViewModel()
        {
            payedServices = new List<Payments>();
            balanceCharges = new List<BalanceCharge>();
            orders = new List<Payments>();
            boughtServices = new List<Payments>();

        }
        public IEnumerable<Payments> boughtServices { get; set; }
        public IEnumerable<Payments>payedServices { get; set; }
        public IEnumerable<Payments> orders { get; set; }
        public IEnumerable<BalanceCharge> balanceCharges { get; set; }
    }
}
