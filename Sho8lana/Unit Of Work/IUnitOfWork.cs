using GraduationProject.Models.Repositories;
using Sho8lana.Models;

namespace Sho8lana.Unit_Of_Work
{
    public interface IUnitOfWork
    {
        public IBaseRepository<Customer> Customers { get;  }
        public IBaseRepository<Service> Services { get;  }
        public IBaseRepository<Contract> Contracts { get;  }
        public IBaseRepository<ServiceMessage> ServiceMessages { get;  }
        public IBaseRepository<Media> Medias { get;  }
        public IBaseRepository<CustomerRequest> CustomerRequests { get;  }
        public IBaseRepository<Category> Categories { get; }

        int complete();
    }
}
