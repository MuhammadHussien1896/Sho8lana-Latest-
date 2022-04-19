namespace Sho8lana.Models.Repositories
{
    public interface IServiceRepository
    {
        IEnumerable<Service> GetAllByCategory(int CategoryId);
    }
}
