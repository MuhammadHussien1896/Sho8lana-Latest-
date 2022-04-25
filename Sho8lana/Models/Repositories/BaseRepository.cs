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
        public T Add(T entity)
        {
            appDbContext.Set<T>().Add(entity);
            return (entity);
            //appDbContext.SaveChanges();
        }

        public async Task Delete(int id)
        {
            T entity = await this.GetById(id);
            appDbContext.Set<T>().Remove(entity);
            //appDbContext.SaveChanges();
        }
        public async Task Delete(string id)
        {
            T entity = await this.GetById(id);
            appDbContext.Set<T>().Remove(entity);
            //appDbContext.SaveChanges();
        }

        public async Task<List<T>> GetAll()
        {
            return await appDbContext.Set<T>().ToListAsync();
        }
        public IEnumerable<T> GetAllSync()
        {
            return appDbContext.Set<T>().ToList();
        }

      

        public async Task<T> GetById(int? id)
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
            T entity = await appDbContext.Set<T>().SingleOrDefaultAsync(expression);
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
