using GraduationProject.Models.Repositories;
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

        public IEnumerable<Service> GetAllByCategory(int CategoryId)
        {
            return appDbContext.Services.Include(s => s.Customer).Where(s => s.CategoryId == CategoryId).ToList();
        }
    }
}
