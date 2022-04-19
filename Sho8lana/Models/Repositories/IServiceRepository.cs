namespace Sho8lana.Models.Repositories
{
    public interface IServiceRepository : IBaseRepository<Service>
    {
        public Task<IEnumerable<Service>> GetAllByCategory(int CategoryId);
    }
}
