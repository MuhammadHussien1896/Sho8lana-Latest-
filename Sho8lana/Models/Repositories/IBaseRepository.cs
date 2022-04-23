﻿using System.Linq.Expressions;

namespace Sho8lana.Models.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        public T Add(T entity);
        public void Update(T entity);
        public Task<T> GetById(int id);
        public Task<T> GetById(string id);
        public Task<T> GetBy(Expression<Func<T, bool>> expression);
        public Task<IEnumerable<T>> GetAllBy(Expression<Func<T, bool>> expression);
        public Task<List<T>> GetAll();
        public IEnumerable<T> GetAllSync();
        public void Delete(int id);
        public void Delete(string id);
        
    }
}
