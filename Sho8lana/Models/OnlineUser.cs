using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sho8lana.Models
{
    public class OnlineUser
    { 
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
    }
}
