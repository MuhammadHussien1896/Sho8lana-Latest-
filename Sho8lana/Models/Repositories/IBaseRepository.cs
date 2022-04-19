using System.Linq.Expressions;

namespace GraduationProject.Models.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        public void Add(T entity);
        public void Update(T entity);
        public T GetById(int id);
        public T GetBy(Expression<Func<T, bool>> expression);
        public List<T> GetAll();
        public void Delete(int id);
        
    }
}
