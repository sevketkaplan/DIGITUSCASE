using System;
using System.Collections.Generic;
using System.Text;
using Core.ResultType;
using DataObject.WM;

namespace DataObject.ViewModels
{
    public class AdminModuleViewModel
    {
        public AdminModuleViewModel()
        {
            AdminDo = new AdminDO();
            ListAdminDo = new List<AdminDO>();
            Pagination = new object();

        }

        public int IsAdmin { get; set; } = 0;// 0 ise admin değil yani kendi email veya username'ni değiştiremez 1 ise admin tüm bilgilerini değiştirebilir
        public int OnlineCount { get; set; } = 0;
        public int OfflineCount { get; set; } = 0;
        public int AccountConfirmCount { get; set; } = 0;
        public int AccountNonConfirmCount { get; set; } = 0;

        public AdminDO AdminDo { get; set; }
        public List<AdminDO> ListAdminDo { get; set; }

        public object Pagination { get; set; }



    }
}
