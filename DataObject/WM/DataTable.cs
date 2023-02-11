using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataObject.WM
{
    public partial class DataTable
    {
        public int RecordsTotal { get; set; } = 0;
        public int RecordsFiltered { get; set; } = 0;
        public Object Data { get; set; } = null;

    }
}
