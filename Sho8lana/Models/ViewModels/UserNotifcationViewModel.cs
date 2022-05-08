namespace Sho8lana.Models.ViewModels
{
    public class UserNotifcationViewModel
    {
        public UserNotifcationViewModel()
        {
            Notifications=new List<Notification>();
        }
        public byte[] customerImage { get; set; }
        public IEnumerable<Notification> Notifications { get; set; }
    }
}
