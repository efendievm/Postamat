using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Postamat.Models;
using System;
using System.Linq;

namespace Postamat.Repositories
{
    /// <summary>
    /// Обобщённый репозиторий.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Repository<T> : IRepository<T> where T : DbEntity
    {
        private DbContext Context;
        public Repository(DbContext context) => Context = context;

        public IQueryable<T> GetAll() => Context.Set<T>();

        public IQueryable<T> GetAll(Func<IQueryable<T>, IIncludableQueryable<T, object>> include) => include(Context.Set<T>());

        public T Get(int Id) => GetAll().FirstOrDefault(m => m.ID == Id);

        public T Get(int Id, Func<IQueryable<T>, IIncludableQueryable<T, object>> include) => GetAll(include).FirstOrDefault(m => m.ID == Id);

        public T Create(T model)
        {

            Context.Set<T>().Add(model);
            Context.SaveChanges();
            return model;
        }

        public T Update(T model)
        {
            Context.Update(model);
            Context.SaveChanges();
            return model;
        }

        public T Delete(int Id)
        {
            var toDelete = Context.Set<T>().FirstOrDefault(m => m.ID == Id);
            if (toDelete != null)
            {
                Context.Set<T>().Remove(toDelete);
                Context.SaveChanges();
            }
            return toDelete;
        }
    }
}