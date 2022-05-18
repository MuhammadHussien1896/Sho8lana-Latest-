using Microsoft.EntityFrameworkCore;
using Sho8lana.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
        public void Delete(T entity)
        {
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
        public async Task<T> GetBy(Expression<Func<T, bool>> expression)
        {
            T entity = await appDbContext.Set<T>().FirstOrDefaultAsync(expression);
            return entity;
        }

        public void Update(T entity)
        {
            appDbContext.Set<T>().Update(entity);

        }

        public async Task<IEnumerable<T>> GetAllBy(Expression<Func<T, bool>> expression)
        {
            IEnumerable<T> entity = await appDbContext.Set<T>().Where(expression).ToListAsync();
            return entity;
        }

        public async Task<IList<T>> GetAllEagerLodingAsync(Expression<Func<T, bool>> expression, string[] includes = null)
        {
            IQueryable<T> query = appDbContext.Set<T>();
            if (includes != null)
            {
                foreach (var item in includes)
                    query = query.Include(item);
            }
            return await query.Where(expression).ToListAsync();

        }
        public async Task<T> GetEagerLodingAsync(Expression<Func<T, bool>> expression, string[] includes = null)
        {
            IQueryable<T> query = appDbContext.Set<T>();
            if (includes != null)
            {
                foreach (var item in includes)
                    query = query.Include(item);
            }
            return await query.SingleOrDefaultAsync(expression);

        }
        public void UpdateList(List<T> entities)
        {
            //try
            //{
            //    foreach (var entityItem in entities)
            //    {
            //        appDbContext.Set<T>().Update(entityItem);
            //    }
            //}
            //catch
            //{
            //    throw new Exception();
            //}
            appDbContext.Set<T>().UpdateRange(entities);
            appDbContext.SaveChanges();

        }

        public async Task<int> Count()
        {
            return await appDbContext.Set<T>().CountAsync();
        }

        public async Task<int> Count(Expression<Func<T, bool>> expression)
        {
            return await appDbContext.Set<T>().CountAsync(expression);
        }

        public async Task<IList<T>> GetAllEagerLodingAsync(Expression<Func<T, bool>> expression,int skip,int take, string[] includes = null)
        {
            IQueryable<T> query = appDbContext.Set<T>();
            if(includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }
            return await query.Where(expression).Skip(skip).Take(take).ToListAsync();

        }
    }
}
