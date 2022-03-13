using Microsoft.EntityFrameworkCore.Query;
using Postamat.Models;
using System;
using System.Linq;

namespace Postamat.Repositories
{
    /// <summary>
    /// Интерфейс обобщённого репозитория.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : DbEntity
    {
        public IQueryable<T> GetAll();
        public IQueryable<T> GetAll(Func<IQueryable<T>, IIncludableQueryable<T, object>> include);
        public T Get(int Id);
        public T Get(int Id, Func<IQueryable<T>, IIncludableQueryable<T, object>> include);
        public T Create(T model);
        public T Update(T model);
        public T Delete(int Id);
    }
}
