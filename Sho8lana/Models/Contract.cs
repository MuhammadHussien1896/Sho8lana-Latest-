using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class Contract
    {
        public int ContractId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDone { get; set; }
        public bool IsCanceled { get; set; }
        [DataType(DataType.Currency)]
        public float ContractPrice { get; set; }
        public int ContractRateStars { get; set; }
        public string ContractRateComment { get; set; }
        public bool ContractRateDone { get; set; }
        public int DeliveryTime { get; set; }
        public bool BuyerAccepted { get; set; }
        public bool BuyerCanceled { get; set; }
        public bool SellerAccepted { get; set; }
        public bool SellerIsDone { get; set; }
        public string BuyerId { get; set; }
        public string SellerId { get; set; }
        public string SericeOwnerId { get; set; }
        public string JobId { get; set; }

        //navigation properties
        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; }
    }
}
