using DataObject.WM;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WM.Extentions
{
    public class Functions
    {
        private static string slashed = Path.DirectorySeparatorChar.ToString();
        public static bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }

        public static bool IsEmail(string email)
        {
            Regex rx = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            bool sonuc = email != null && email != "" ? rx.IsMatch(email) : false;
            return sonuc;

        }
        public static string GenerateCode()
        {
            string value = ""; //Boş bir değer tanımlıyoruz.
            Random rnd = new Random(); // Burada Rastgele değeri tanımlıyouz.
            for (int c = 0; c < 6; c++) //25 haneli rakam-harf üretmek için döngü yaptık.
            {
                int ck = rnd.Next(0, 2); // 0 veya 1
                if (ck == 0) // Rastgele üretilen sayı 0 ise sayı üret.
                {
                    int num = rnd.Next(1, 10);
                    value += num.ToString();
                }
                else // Değilse harf üret (65 ile 91 arası ascii kodlar olduğu için rakam değerleri girdik.)
                {
                    int x = rnd.Next(65, 91);
                    char chr = Convert.ToChar(x); //ascii kod olarak üretilen sayıyı harfe çevirdik.
                    value += chr; //Değere atadık.
                }
            }
            return value;
        }
        public static void WriteToFile(string DirectoryPath, string FileName, string Text)
        {
            //Check Whether directory exist or not if not then create it
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            string FilePath = DirectoryPath + slashed + FileName;
            //Check Whether file exist or not if not then create it new else append on same file
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, Text);
            }
            else
            {
                //Text = $"{Environment.NewLine}{Text}";
                File.WriteAllText(FilePath, Text);
            }
        }
        public static string ReadFromFile(string DirectoryPath, string FileName)
        {
            string result = "";
            if (Directory.Exists(DirectoryPath))
            {
                string FilePath = DirectoryPath + slashed + FileName;
                if (File.Exists(FilePath))
                {
                    result = File.ReadAllText(FilePath, Encoding.UTF8);
                }
            }
            return result;
        }

        public static string MenuRead(string path, string file)
        {
            return ReadFromFile(path, file);
        }

        public static string HtmlStringBuilderClear(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            url = url.Trim();
            url = url.Replace("\r", "");
            url = url.Replace("\n", "");
            url = url.Replace("\t", "");
            url = url.Replace("  ", " ");

            return url;
        }
        public static string FriendlyUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            url = url.ToLower();
            url = url.Trim();
            if (url.Length > 100)
            {
                url = url.Substring(0, 100);
            }
            url = url.Replace("İ", "I");
            url = url.Replace("ı", "i");
            url = url.Replace("ğ", "g");
            url = url.Replace("Ğ", "G");
            url = url.Replace("ç", "c");
            url = url.Replace("Ç", "C");
            url = url.Replace("ö", "o");
            url = url.Replace("Ö", "O");
            url = url.Replace("ş", "s");
            url = url.Replace("Ş", "S");
            url = url.Replace("ü", "u");
            url = url.Replace("Ü", "U");
            url = url.Replace("'", "");
            url = url.Replace("\"", "");
            char[] replacerList = @"$%#@!*?;:~`+=()[]{}|\'<>,/^&"".".ToCharArray();
            for (int i = 0; i < replacerList.Length; i++)
            {
                string strChr = replacerList[i].ToString();
                if (url.Contains(strChr))
                {
                    url = url.Replace(strChr, string.Empty);
                }
            }
            Regex r = new Regex("[^a-zA-Z0-9_-]");
            url = r.Replace(url, "-");
            while (url.IndexOf("--") > -1)
                url = url.Replace("--", "-");
            return url;
        }

        public static async Task<string> MakeNotification(string[] player_ids, IConfiguration configuration, string message = "")
        {
            try
            {
                HttpClient client = new HttpClient();
                var token1 = new
                {
                    app_id = configuration.GetSection("OneSignal").GetSection("app_id").Value,
                    contents = new { en = message },
                    headings = new { en = "sevketkaplan" },
                    include_player_ids = player_ids
                };

                var json = JsonConvert.SerializeObject(token1);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.PostAsync("https://onesignal.com/api/v1/notifications", data);
                var responseString = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(responseString);
                return "1";
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }

        // ====================== Pagination Algorithm Version-2 (advenced algorithm) ======================
        public static object GetPaginateInfo(int contentListCount, int page = 1, int pagesize = 10)
        {
            int current = 0, prev = 0, next = 0;
            List<object> rangeWithDots = new List<object>();
            if (page > 0 && pagesize > 0)
            {
                current = page;
                int items = 0;
                if (contentListCount % pagesize == 0)
                    items = contentListCount / pagesize;
                else
                    items = (contentListCount / pagesize) + 1;
                prev = current == 1 ? -1 : current - 1;
                next = current == items ? -1 : current + 1;

                if ((contentListCount / pagesize) + 1 > 0)
                {
                    int last = contentListCount;
                    int delta = 2, l = 0;
                    int left = current - delta;
                    int right = current + delta + 1;

                    List<int> range = new List<int>();
                    rangeWithDots = new List<object>();
                    for (int i = 1; i < last; i++)
                    {
                        if (i == 1 || i == last || i >= left && i < right)
                            range.Add(i);
                    }

                    foreach (var i in range)
                    {
                        if (l > 0)
                        {
                            if (i - l == 2)
                                rangeWithDots.Add(l + 1);
                            else if (i - l != 1)
                                rangeWithDots.Add("...");
                        }
                        rangeWithDots.Add(i);
                        l = i;
                    }
                }
            }
            return new { current = current, prev = prev, next = next, pages = rangeWithDots };
        }

        // ====================== Pagination Algorithm Version-1 (base algorithm) ======================

        //public static object GetPaginateInfo(int contentListCount, int page = 1, int pagesize = 10)
        //{
        //    int currentPageNumber = 0;
        //    int prev = 0;
        //    int next = 0;
        //    int[] pages = { };

        //    if (page > 0 && pagesize > 0)
        //    {
        //        currentPageNumber = page;
        //        int items = 0;
        //        if (contentListCount % pagesize == 0)
        //            items = contentListCount / pagesize;
        //        else
        //            items = (contentListCount / pagesize) + 1;
        //        prev = currentPageNumber == 1 ? -1 : currentPageNumber - 1;
        //        next = currentPageNumber == items ? -1 : currentPageNumber + 1;
        //        if ((contentListCount / pagesize) + 1 > 0)
        //        {
        //            pages = new int[items];
        //            for (int i = 0; i < items; i++)
        //                pages[i] = i + 1;
        //        }
        //    }
        //    return new { currentPageNumber = currentPageNumber, prev = prev, next = next, pages = pages };
        //}

    }


}
