using System.Linq.Expressions;

namespace Sho8lana.Models.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        public void Add(T entity);
        public void Update(T entity);
        public Task<T> GetById(int id);
        public Task<T> GetBy(Expression<Func<T, bool>> expression);
        public Task<List<T>> GetAll();
    
        public void Delete(int id);
        
    }
}
