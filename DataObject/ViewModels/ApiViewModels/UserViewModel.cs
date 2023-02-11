using System;
using System.Collections.Generic;
using System.Text;

namespace DataObject.WM.ApiViewModels
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            AdminDO = new AdminDO();
          

        }

        public AdminDO AdminDO { get; set; }
       
    }
}
