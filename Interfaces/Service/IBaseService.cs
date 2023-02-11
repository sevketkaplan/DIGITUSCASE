using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DataAccessLayer.WMDbContext;

namespace Interfaces.Service
{
    public interface IBaseService<T> where T : class
    {
        List<T> GetAll();
        List<T> GetAsQueryable(Expression<Func<T, bool>> predicate = null);
        T Get(Expression<Func<T, bool>> predicate = null);
        void Update(T model);
        void Add(T model);
        void Delete(T model);
        bool Exist(Expression<Func<T, bool>> predicate);
    }
}
