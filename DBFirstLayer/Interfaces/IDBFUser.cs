using Core.ResultType;
using DataObject.WM;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBFirstLayer.Interfaces
{
    public interface IDBFUser
    {
        Result<AdminDO> GetByToken(string token);
    }
}
