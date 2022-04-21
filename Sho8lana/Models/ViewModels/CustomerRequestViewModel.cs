namespace Sho8lana.Models.ViewModels
{
    public class CustomerRequestViewModel
    {
        public CustomerRequestViewModel()
        {
            IncomingRequests = new List<CustomerRequest>();
            OutgoingRequests = new List<CustomerRequest>();
        }
        public ICollection<CustomerRequest> IncomingRequests { get; set; }
        public ICollection<CustomerRequest> OutgoingRequests { get; set; }
    }
}
