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
    public class AdminService : BaseService<Admin>, IAdminService
    {
        //public List<Admin> GetAll()
        //{
        //    using (WMDbContext context = new WMDbContext())
        //    {
        //        //return context.Set<Admin>().ToList();
        //        return context.Set<Admin>().AsNoTracking().ToList();
        //    }
        //}

        public List<Admin> GetAsQueryable(Expression<Func<Admin, bool>> predicate = null)
        {
            using (WMDbContext context = new WMDbContext())
            {
                if (predicate == null)
                    return context.Set<Admin>()
                        .AsNoTracking().ToList();
                else
                    return context.Set<Admin>().Where(predicate)
                        .AsNoTracking().ToList();
            }
        }


        //public Admin Get(Expression<Func<Admin, bool>> predicate = null)
        //{
        //    using (WMDbContext context = new WMDbContext())
        //    {
        //        return context.Set<Admin>().AsNoTracking().FirstOrDefault(predicate ?? throw new ArgumentNullException(nameof(predicate)));
        //    }
        //}

        //public void Update(Admin entity)
        //{
        //    using (WMDbContext context = new WMDbContext())
        //    {
        //        var updatedEntity = context.Entry(entity);
        //        updatedEntity.State = EntityState.Modified;
        //        context.SaveChanges();
        //    }
        //}

        //public void Add(Admin entity)
        //{
        //    using (WMDbContext context = new WMDbContext())
        //    {
        //        var addEntity = context.Entry(entity);
        //        addEntity.State = EntityState.Added;
        //        context.SaveChanges();

        //    }
        //}

        //public void Delete(Admin entity)
        //{
        //    using (WMDbContext context = new WMDbContext())
        //    {
        //        var deleteEntity = context.Entry(entity);
        //        deleteEntity.State = EntityState.Deleted;
        //        context.SaveChanges();
        //    }
        //}

        //public bool Exist(Expression<Func<Admin, bool>> predicate)
        //{
        //    using (WMDbContext context = new WMDbContext())
        //    {
        //        return context.Set<Admin>().AsNoTracking().Any(predicate);
        //    }
        //}

    }
}
