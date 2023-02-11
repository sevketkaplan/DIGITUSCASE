using System;
using System.Collections.Generic;
using System.Text;
using Core.ResultType;
using DataObject.ViewModels;
using DataObject.WM;
using Utility.Security.Jwt;

namespace Interfaces.Business
{
    public interface IAdminBL
    {

        Result<List<AdminDO>> GetAll();
        Result<List<AdminDO>> GetAll(int usertype);
        Result<AdminDO> GetById(int adminId);
        Result<List<AdminDO>> GetByEmail(string email);
        Result<AdminDO> Add(AdminDO model);
        Result<AdminDO> Update(AdminDO model);
        Result<AdminDO> UpdatePassword(AdminDO model);
        Result<AdminDO> Delete(AdminDO model);
        Result<AdminDO> Login(string username, string password);
        Result<AdminDO> GetByEmailResetPassword(string email);
        Result<bool> SendAccountConfiremdEmail(AdminDO adminDO);
        Result<bool> SendResetPasswordEmail(AdminDO adminDO);
        Result<DataTable> GetUnConfirmedListFilteredData(object dynamic);
        Result<AdminDO> GetByToken(string token);
        Result<AdminDO> UpdateState(string token,bool state);
        Result<AdminModuleViewModel> GetCounts();
    }
}
