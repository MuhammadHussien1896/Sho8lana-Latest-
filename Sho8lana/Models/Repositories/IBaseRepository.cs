using System.Linq.Expressions;

namespace Sho8lana.Models.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        public T Add(T entity);
        public void Update(T entity);
        public Task<T> GetById(int? id);
        public Task<T> GetById(string id);
        public Task<T> GetBy(Expression<Func<T, bool>> expression);
        public Task<IEnumerable<T>> GetAllBy(Expression<Func<T, bool>> expression);
        public Task<List<T>> GetAll();
        public IEnumerable<T> GetAllSync();
        public Task Delete(int id);
        public Task Delete(string id);

        public void Delete(T entity);



        public Task<IList<T>> GetAllEagerLodingAsync(Expression<Func<T, bool>> expression, string[] includes = null);

        public Task<T> GetEagerLodingAsync(Expression<Func<T, bool>> expression, string[] includes = null);
        public void UpdateList(List<T> entity);

    }
}
