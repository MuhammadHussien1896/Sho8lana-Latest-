namespace Sho8lana.Models
{
    public class Category
    {
        public Category()
        {
            Services = new List<Service>();
        }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string CategoryImg { get; set; }
        public string Description { get; set; }
        public ICollection<Service> Services { get; set; }
    }
}
