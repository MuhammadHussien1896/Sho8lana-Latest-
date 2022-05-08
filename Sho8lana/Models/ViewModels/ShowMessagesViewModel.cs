namespace Sho8lana.Models.ViewModels
{
    public class ShowMessagesViewModel
    {
        public ShowMessagesViewModel()
        {
            Messages = new List<ServiceMessage>();
        }
        public ICollection<ServiceMessage> Messages { get; set; }
        public string BuyerName { get; set; }
        public string SellerName { get; set; }
        public string BuyerId { get; set; }
        public string SellerId { get; set; }
        public int ServiceId { get; set; }
    }
}
