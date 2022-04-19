using Sho8lana.Data;
using System.Linq.Expressions;

namespace GraduationProject.Models.Repositories
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

        public void Delete(int id)
        {
            T entity = this.GetById(id);
            appDbContext.Set<T>().Remove(entity);
            //appDbContext.SaveChanges();
        }

        public List<T> GetAll()
        {
            return appDbContext.Set<T>().ToList();
        }

        public T GetById(int id)
        {
            T entity = appDbContext.Set<T>().Find(id);
            return entity;
        }
        public T GetBy(Expression<Func<T,bool>> expression)
        {
            T entity = appDbContext.Set<T>().Single(expression);
            return entity;
        }

        public void Update(T entity)
        {
            appDbContext.Set<T>().Update(entity);
            //appDbContext.SaveChanges();
        }
    }
}
