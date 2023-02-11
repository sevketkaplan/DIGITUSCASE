using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WM.Models
{
    public class FileResponse
    {
        public FileResponse(string name,string message, string ex="")
        {
            FileName = name;
            Message = message;
            EX = ex;
        }
        public string FileName { get; set; }
        public string Message { get; set; }
        public string EX { get; set; }
    }
}
