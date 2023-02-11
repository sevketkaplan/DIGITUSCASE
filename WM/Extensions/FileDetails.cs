using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WM.Extentions
{
    public class FileDetails
    {
        public int width { get; set; }
        public int height { get; set; }
        public double size { get; set; }
        public string address { get; set; }
        public string extention { get; set; }
        public string fullpathname { get; set; }
        internal FileDetails() { }
        internal FileDetails(string fpn,int w, int h, int s, string a,string ex)
        {
            this.width = w;
            this.height = h;
            this.address = a;
            this.size = s;
            this.extention = ex;
            this.fullpathname = fpn;
        }
    }
}
