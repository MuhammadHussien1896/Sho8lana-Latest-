using Sho8lana.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using Sho8lana.Data;

namespace Sho8lana.Models.Repositories
{
    public class ServiceRepository : BaseRepository<Service>, IServiceRepository
    {
        private readonly ApplicationDbContext appDbContext;

        public ServiceRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Service>> GetAllByCategory(int CategoryId)
        {
            return await appDbContext.Services.Include(s => s.Customer).Where(s => s.CategoryId == CategoryId).ToListAsync();
        }
    }
}
