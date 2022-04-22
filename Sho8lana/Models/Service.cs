using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class Service
    {
        public Service()
        {
            PublishDate         = DateTime.Now;
            Medias              = new List<Media>();
            CustomerRequests    = new List<CustomerRequest>();
            Contracts           = new List<Contract>();
            ServiceMessages     = new List<ServiceMessage>();
        }
        
        public int ServiceId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required,StringLength(50,MinimumLength =5,ErrorMessage ="Title must be between 5-25 characters")]
        public string Title { get; set; }
        [DataType(DataType.Currency)]
        public float Price { get; set; }
        public string CustomerInstructions { get; set; }
        public bool IsCash { get; set; }
        public bool IsFreelancer { get; set; }
        public DateTime PublishDate { get; set; }
        public bool IsAccepted { get; set; }

        public float Rate { get; set; }

        //navigation Properities
        public virtual ICollection<Media> Medias { get; set; }
        
        [ForeignKey("Category"),Required(ErrorMessage ="Category is Required")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }


        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public virtual ICollection<CustomerRequest> CustomerRequests { get; set; }

        public virtual ICollection<Contract> Contracts { get; set; }
        public virtual ICollection<ServiceMessage> ServiceMessages { get; set; }

    }
}
