using Microsoft.EntityFrameworkCore;
using Sho8lana.Data;
using System.Linq.Expressions;

namespace Sho8lana.Models.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext appDbContext;

        public BaseRepository(ApplicationDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        public void Add(T entity)
        {
            appDbContext.Set<T>().Add(entity);
            //appDbContext.SaveChanges();
        }

        public async void Delete(int id)
        {
            T entity = await this.GetById(id);
            appDbContext.Set<T>().Remove(entity);
            //appDbContext.SaveChanges();
        }
        public async void Delete(string id)
        {
            T entity = await this.GetById(id);
            appDbContext.Set<T>().Remove(entity);
            //appDbContext.SaveChanges();
        }

        public async Task<List<T>> GetAll()
        {
            return await appDbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            T entity = await appDbContext.Set<T>().FindAsync(id);
            return entity;
        }
        public async Task<T> GetById(string id)
        {
            T entity = await appDbContext.Set<T>().FindAsync(id);
            return entity;
        }
        public async Task<T> GetBy(Expression<Func<T,bool>> expression)
        {
            T entity = await appDbContext.Set<T>().SingleAsync(expression);
            return entity;
        }

        public void Update(T entity)
        {
            appDbContext.Set<T>().Update(entity);
            //appDbContext.SaveChanges();
        }

        public async Task<IEnumerable<T>> GetAllBy(Expression<Func<T, bool>> expression)
        {
            IEnumerable<T> entity = await appDbContext.Set<T>().Where(expression).ToListAsync();
            return entity;
        }
    }
}
