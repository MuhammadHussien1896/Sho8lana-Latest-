using GraduationProject.Models.Repositories;
using Sho8lana.Data;
using Sho8lana.Models;

namespace Sho8lana.Unit_Of_Work
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            Customers = new BaseRepository<Customer>(dbContext);
            Services = new BaseRepository<Service>(dbContext);
            Contracts = new BaseRepository<Contract>(dbContext);
            ServiceMessages = new BaseRepository<ServiceMessage>(dbContext);
            Medias = new BaseRepository<Media>(dbContext);
            CustomerRequests = new BaseRepository<CustomerRequest>(dbContext);
            Categories = new BaseRepository<Category>(dbContext);
        }
        public IBaseRepository<Customer> Customers { get; private set; }
        public IBaseRepository<Service> Services { get; private set; }
        public IBaseRepository<Contract> Contracts { get; private set; }
        public IBaseRepository<ServiceMessage> ServiceMessages { get; private set; }
        public IBaseRepository<Media> Medias { get; private set; }
        public IBaseRepository<CustomerRequest> CustomerRequests { get; private set; }
        public IBaseRepository<Category> Categories { get; private set; }

        public int complete()
        {
            return dbContext.SaveChanges();
        }
    }
}
