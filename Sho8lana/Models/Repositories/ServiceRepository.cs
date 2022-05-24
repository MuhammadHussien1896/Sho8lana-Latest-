using Sho8lana.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using Sho8lana.Data;
using Sho8lana.DTOs;

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
            return await appDbContext.Services.Include(s => s.Customer).Include(s=>s.Medias).Where(s => s.CategoryId == CategoryId).ToListAsync();
        }
        public async Task<IQueryable<Service>> Search(ServiceSearchProperties serviceSearchProperties)
        {
            var SearchResult = appDbContext.Services.AsQueryable();
            if(serviceSearchProperties != null)
            {
                if (serviceSearchProperties.Type!=null)
                    SearchResult = SearchResult.Where(
                        Service => Service.IsFreelancer == serviceSearchProperties.Type)
                        .OrderByDescending(Service=>Service.PublishDate);

                if (!String.IsNullOrEmpty(serviceSearchProperties.Searchtext))
                    SearchResult=SearchResult.Where(
                        Service=>Service.Title.Contains(serviceSearchProperties.Searchtext)||
                                 Service.Description.Contains(serviceSearchProperties.Searchtext));

                if(serviceSearchProperties.CategoryId!=null)
                    SearchResult=SearchResult.Where(
                       Service=>Service.CategoryId==serviceSearchProperties.CategoryId);

                if(serviceSearchProperties.ServiceRate!=null)
                    SearchResult=SearchResult.Where(
                        Service=>Service.Rate<=serviceSearchProperties.ServiceRate)
                        .OrderByDescending(Service=>Service.Rate);

                if (serviceSearchProperties.ServicePrice != null)
                    SearchResult = SearchResult.Where(
                        Service => Service.Price <= serviceSearchProperties.ServicePrice)
                        .OrderByDescending(Service=>Service.Price);

                if (serviceSearchProperties.Trusted != null)
                    SearchResult = SearchResult.Where(
                        Service => Service.Customer.IsVerified == serviceSearchProperties.Trusted);

                if (serviceSearchProperties.Skip != null)
                    SearchResult = SearchResult.Skip((int)serviceSearchProperties.Skip);

                if (serviceSearchProperties.Take != null)
                    SearchResult = SearchResult.Take((int)serviceSearchProperties.Take);

            }
            return SearchResult;
        }
    }
}
