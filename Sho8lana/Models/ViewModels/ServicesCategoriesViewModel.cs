namespace Sho8lana.Models.ViewModels
{
    public class ServicesCategoriesViewModel
    {
        public ServicesCategoriesViewModel()
        {
            this.services = new List<Service>();
            this.categories = new List<Category>();
        }

        public ICollection<Service> services { get; set; }
        public ICollection<Category> categories { get; set; }
    }
}
