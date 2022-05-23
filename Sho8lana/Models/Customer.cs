using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class Customer : IdentityUser
    {
        public Customer()
        {
            IsPremium           = false;
            RegisterationDate   = DateTime.Today;
            IsVerified          = false;
            Balance             = 0.0f;
            Services            = new List<Service>();
            CustomerRequests    = new List<CustomerRequest>();
            Contracts           = new List<Contract>();
            ServiceMessages     = new List<ServiceMessage>();
            Notifications       = new List<Notification>();
            ProfilePicture      = null;
        }

        [RegularExpression(@"[a-zA-z|\u0621-\u064A]+", ErrorMessage = "من فضلك ادلخل الإسم بطريقة صحيحة ")]
        [Required, StringLength(15, MinimumLength = 3, ErrorMessage = "يجب أن يكون الإسم الأول لا يقل عن 3 حروف أو يذيد عن 15 حرف ")]
        public string FirstName { get; set; }
        [Required, StringLength(15, MinimumLength = 3, ErrorMessage = "يجب أن يكون الإسم الثانى لا يقل عن 3 حروف أو يذيد عن 15 حرف ")]
        [RegularExpression(@"[a-zA-z|\u0621-\u064A]+", ErrorMessage = "من فضلك ادلخل الإسم بطريقة صحيحة ")]
        public string LastName { get; set; }
        
        [Required, StringLength(50, MinimumLength = 3)]
        public string Country { get; set; }
        [Required, StringLength(20, MinimumLength = 3)]
        public string City { get; set; }
        [Required, StringLength(25, MinimumLength = 3)]
        public string Area { get; set; }
        [Required,MaxLength(6)]
        public string Gender { get; set; }
        public bool IsPremium  { get; set; }

        public DateTime RegisterationDate { get; set; }
        
        public string AboutMe { get; set; }
        
        public string ProfileImage { get; set; }
        public byte[] ProfilePicture { get; set; }
        //image
        public string NationalIdImage { get; set; }

        public byte[] NationalIdPicture { get; set; }

        public bool IsVerified { get; set; }
        [DataType(DataType.Currency)]
        public float Balance { get; set; }
        public float PendingBalance { get; set; }
        
        //navigation properties
        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<CustomerRequest> CustomerRequests { get; set; }
        public virtual ICollection<Contract> Contracts { get; set; }
        public virtual ICollection<ServiceMessage> ServiceMessages { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }

    }
    
}
