using DataAccessLayer.WMDbContext;
using Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Services
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        public List<T> GetAll()
        {
            using (WMDbContext context = new WMDbContext())
            {
                return context.Set<T>().AsNoTracking().ToList();
            }
        }

        public List<T> GetAsQueryable(Expression<Func<T, bool>> predicate = null)
        {
            using (WMDbContext context = new WMDbContext())
            {
                if (predicate == null)
                    return context.Set<T>()
                        .AsNoTracking().ToList();
                else
                    return context.Set<T>().Where(predicate)
                        .AsNoTracking().ToList();
            }
        }

        public T Get(Expression<Func<T, bool>> predicate = null)
        {
            using (WMDbContext context = new WMDbContext())
            {
                return context.Set<T>().AsNoTracking().FirstOrDefault(predicate ?? throw new ArgumentNullException(nameof(predicate)));
            }
        }

        public void Update(T entity)
        {
            using (WMDbContext context = new WMDbContext())
            {
                var updatedEntity = context.Entry(entity);
                updatedEntity.State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void Add(T entity)
        {
            using (WMDbContext context = new WMDbContext())
            {
                var addEntity = context.Entry(entity);
                addEntity.State = EntityState.Added;
                context.SaveChanges();

            }
        }

        public void Delete(T entity)
        {
            using (WMDbContext context = new WMDbContext())
            {
                var deleteEntity = context.Entry(entity);
                deleteEntity.State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        public bool Exist(Expression<Func<T, bool>> predicate)
        {
            using (WMDbContext context = new WMDbContext())
            {
                return context.Set<T>().AsNoTracking().Any(predicate);
            }
        }


    }
}
