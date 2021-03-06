using Sho8lana.Paging;

namespace Sho8lana.Models.ViewModels
{
    public class ServiceDisplay
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public float? Price { get; set; }
        public bool? IsFreelance { get; set; }
        public string CreatedDate { get; set; }
        public bool? IsCash { get; set; }
        public int? BuyersCount { get; set; }
        public string customerId { get; set; }
        public string ServiceHeader { get; set; }
        public byte[] CustomerImage { get; set; }
        public float? Rate { get; set; }
    }
}
