using Sho8lana.Data;
using Sho8lana.Models;
using Sho8lana.Models.Repositories;

namespace Sho8lana.Unit_Of_Work
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            Customers = new BaseRepository<Customer>(dbContext);
            Services = new ServiceRepository(dbContext);
            Contracts = new BaseRepository<Contract>(dbContext);
            ServiceMessages = new BaseRepository<ServiceMessage>(dbContext);
            Medias = new BaseRepository<Media>(dbContext);
            CustomerRequests = new BaseRepository<CustomerRequest>(dbContext);
            Categories = new BaseRepository<Category>(dbContext);
            Governorates = new BaseRepository<Governorate>(dbContext);
            Cities = new BaseRepository<City>(dbContext);
            Notifications = new BaseRepository<Notification>(dbContext);
        }
        public IBaseRepository<Customer> Customers { get; private set; }
        public IServiceRepository Services { get; private set; }
        public IBaseRepository<Contract> Contracts { get; private set; }
        public IBaseRepository<ServiceMessage> ServiceMessages { get; private set; }
        public IBaseRepository<Media> Medias { get; private set; }
        public IBaseRepository<CustomerRequest> CustomerRequests { get; private set; }
        public IBaseRepository<Category> Categories { get; private set; }
        public IBaseRepository<Governorate> Governorates { get; private set; }
        public IBaseRepository<City> Cities { get; private set; }
        public IBaseRepository<Notification> Notifications { get; private set; }

        public async Task<int> complete()
        {
            return await dbContext.SaveChangesAsync();
        }
    }
}
