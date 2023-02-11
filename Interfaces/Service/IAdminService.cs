using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using DataAccessLayer.WMDbContext;

namespace Interfaces.Service
{
    public interface IAdminService: IBaseService<Admin>
    {
        //List<Admin> GetAll();
        //List<Admin> GetAsQueryable(Expression<Func<Admin, bool>> predicate = null);      
        //Admin Get(Expression<Func<Admin, bool>> predicate = null);
        //void Update(Admin model);
        //void Add(Admin model);
        //void Delete(Admin model);
        //bool Exist(Expression<Func<Admin, bool>> predicate);
    }
}
