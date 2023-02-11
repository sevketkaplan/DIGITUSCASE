using Core.ResultType;
using DataObject.ViewModels;
using DataObject.WM;
using Interfaces.Business;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WM.Extensions;
using WM.Extentions;

namespace WM.Controllers
{
    public class AdminController : Controller
    {

        private IAdminBL _service;
        private IHostingEnvironment _env;
        private IConfiguration _configurationBL;
        private readonly IHubContext<WMHub> _orderhub;

        public AdminController(IConfiguration configuration,
            IAdminBL service, IHostingEnvironment hostingEnvironment,
              IHubContext<WMHub> orderhub)
        {
            _orderhub = orderhub;
            _configurationBL = configuration;
            _service = service;
            _env = hostingEnvironment;



        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(AdminDO collection)
        {
            Result<AdminModuleViewModel> result;
            AdminModuleViewModel model = new AdminModuleViewModel();
            try
            {
                collection.CreateAt = DateTime.Now;
                collection.Name = collection.Name.Trim();
                collection.Surname = collection.Surname.Trim();
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(collection.Email);
                if (!match.Success)
                {
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = collection,
                    };
                    result = new Result<AdminModuleViewModel>(model);
                    ViewBag.ResultState = new { success = true, icon = "warning", message = "Girmiş olduğunuz Email geçersizdir!" };
                    return View(result);
                }
                if (collection.Password.Length < 8)
                {
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = collection,
                    };
                    result = new Result<AdminModuleViewModel>(model);
                    ViewBag.ResultState = new { success = true, icon = "warning", message = "Şifrenin uzunluğu en az 8 karakter olmalıdır.!" };
                    return View(result);
                }
                var state = _service.Add(collection);

                if (state.IsSuccess)
                {
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = collection,
                    };
                    result = new Result<AdminModuleViewModel>(model);
                    ViewData["message"] = "Kayıt işlemi başarılı. Lütfen Mailinize gelen bağlantı ile hesabınızı onaylayınız.";
                    return View("~/Views/Admin/AccountRegisterSuccess.cshtml");
                }
                else
                {
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = collection,

                    };
                    result = new Result<AdminModuleViewModel>(model);
                    ViewBag.ResultState = new { success = true, icon = "warning", message = state.Message };
                    return View(result);
                }
            }
            catch (Exception ex)
            {
                result = new Result<AdminModuleViewModel>(false, ex.Message);
                return View(result);
            }
        }
        // ================== <Start> Login ve Logout İşlemleri ==================
        [CheckAuth("Admin", "Login", false)]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [CheckAuth("Admin", "Login", false)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminDO adminDo)
        {
            Result<AdminDO> giris = new Result<AdminDO>();
            try
            {
                var girisIslemi = _service.Login(adminDo.Email, adminDo.Password);
                if (girisIslemi.IsSuccess && girisIslemi.Data != null)
                {
                    if (girisIslemi.Data.AccountConfirm == false)
                    {
                        ViewData["message"] = "Bu hesap Aktif Değil";

                        return Redirect($"https://localhost:5001/Admin/AccountConfirmed?t={girisIslemi.Data.Token}&c={girisIslemi.Data.ConfirmCode}");
                    }


                    int days = 365;

                    CookieOptions tokenCookie = new CookieOptions();
                    tokenCookie.IsEssential = true;
                    tokenCookie.Expires = DateTime.UtcNow.Date.AddDays(days);
                    Response.Cookies.Append("usertoken", girisIslemi.Data.Token, tokenCookie);

                    CookieOptions vid = new CookieOptions();
                    vid.IsEssential = true;
                    vid.Expires = DateTime.UtcNow.Date.AddDays(days);
                    int adminId = girisIslemi.Data.Id;
                    var cryption = adminId.ToString() + "-" + DateTime.UtcNow.Date.AddDays(days);
                    var idEncryption = Encryption.Encrypt(cryption);
                    Response.Cookies.Append("vid", idEncryption, vid);

                    var User = _service.GetById(Int32.Parse(girisIslemi.Data.Id.ToString())).Data; // YENİ VERSİYON

                    string location_name = "";

                    string userTxt = "";
                    userTxt += "<div class=\"media-body\"><div class=\"media-title font-weight-semibold\">" + User.Name + " " + User.Surname + "</div><div class=\"font-size-xs opacity-50\"></div><div class=\"font-size-xs opacity-50\">" + location_name + "</div></div>";

                    _service.UpdateState(girisIslemi.Data.Token, true);
                    Functions.WriteToFile(_env.WebRootPath + "/usermenu/", adminId + ".html", userTxt);
                    if (!String.IsNullOrEmpty(HttpContext.Request.Query["returnURL"]))
                    {
                        //ViewData["message"] = "Silme işlemi tamamlandı.";
                        //var a = HttpContext.Request.Query["returnURL"];
                        return Redirect(HttpContext.Request.Query["returnURL"]);
                    }

                    return RedirectToAction("Index");


                }
                else
                {
                    ViewData["message"] = "Giriş İşlemi Başarısız.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewData["message"] = "Bilinmeyen Bir Hata, Lütfen İnternetinizi Kontrol Ediniz.";
                return View();
            }

        }
        public ActionResult Logout()
        {
            //CookieOptions usernameCookie = new CookieOptions();
            //usernameCookie.IsEssential = true;
            //usernameCookie.Expires = DateTime.UtcNow.AddMonths(-1);
            //Response.Cookies.Append("vusername", "", usernameCookie);
            string authtoken = HttpContext.Request.Cookies["usertoken"];
            int days = -365;
            _service.UpdateState(authtoken, false);
            CookieOptions vid = new CookieOptions();
            vid.IsEssential = true;
            vid.Expires = DateTime.UtcNow.AddDays(days);
            Response.Cookies.Append("vid", "", vid);

            CookieOptions token = new CookieOptions();
            token.IsEssential = true;
            token.Expires = DateTime.UtcNow.AddDays(days);
            Response.Cookies.Append("usertoken", "", token);


            bool state = false;

            if (string.IsNullOrEmpty(HttpContext.Request.Cookies["vid"]))
                state = true;
            if (state)
                return Redirect("/si_panel");
            else
            {
                ViewData["message"] = "Çıkış işlemi yapılamadı";
                return Redirect("/Admin/Index");
            }

        }

        // ================== </End> Login ve Logout İşlemleri ==================

        /*
         */

        // ================== <Start> After Login which Profile opened ==================
        // 

        [CheckAuth]
        public ActionResult Index()
        {
            Result<AdminDO> user = new Result<AdminDO>();
            AdminModuleViewModel model = new AdminModuleViewModel();
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>(false, "");
            try
            {
                string token = !string.IsNullOrEmpty(Request.Cookies["usertoken"].ToString()) ? Request.Cookies["usertoken"].ToString() : null;
                if (token != null)
                {
                    user = _service.GetByToken(token);
                    if (user.IsSuccess && user.Data != null)
                    {
                        var countData = _service.GetCounts();
                        if (!countData.IsSuccess)
                        {
                            result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "Öyle Bir Kullanıcı Bulunmadı.");
                        }
                        // ============== </End> Get Chart Data Area ==============
                        model = new AdminModuleViewModel
                        {
                            OnlineCount = countData.Data.OnlineCount,
                            OfflineCount = countData.Data.OfflineCount,
                            AccountConfirmCount = countData.Data.AccountConfirmCount,
                            AccountNonConfirmCount = countData.Data.AccountNonConfirmCount,
                            AdminDo = user.Data,
                        };
                        result = new Result<AdminModuleViewModel>(true, ResultTypeEnum.Success, model, "");
                    }
                    else
                    {
                        result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "Öyle Bir Kullanıcı Bulunmadı.");
                    }
                }
                else
                {
                    result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "Öyle Bir Kullanıcı Bulunmadı.");
                }

            }
            catch (Exception ex)
            {
                result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "" + ex.Message);
            }
            return View(result);
        }



        [CheckAuth]
        public ActionResult Create()
        {
            ViewBag.ResultState = null;
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>();
            AdminModuleViewModel model = new AdminModuleViewModel();
            try
            {
                string token = !string.IsNullOrEmpty(Request.Cookies["usertoken"].ToString()) ? Request.Cookies["usertoken"].ToString() : null;
                var admin = _service.GetByToken(token);
                if (!admin.IsSuccess || admin.Data == null || admin.Data.Id < 1)
                {
                    return RedirectToAction("Logout");
                }
            }
            catch (Exception ex)
            {

                result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "" + ex.Message);
            }

            return View(result);
        }

        [CheckAuth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminDO collection)
        {
            Result<AdminModuleViewModel> result;
            AdminModuleViewModel model = new AdminModuleViewModel();

            try
            {
                string token = Request.Cookies["usertoken"] != null && !string.IsNullOrEmpty(Request.Cookies["usertoken"].ToString()) ? Request.Cookies["usertoken"].ToString() : null;
                if (token == null)
                {
                    return RedirectToAction("Logout");
                }
                var admin = _service.GetByToken(token);
                if (!admin.IsSuccess || admin.Data == null || admin.Data.Id < 1)
                {
                    return RedirectToAction("Logout");
                }


                collection.CreateAt = DateTime.Now;
                collection.Name = collection.Name.Trim();
                collection.Surname = collection.Surname.Trim();

                if (collection.Password.Length < 8)
                {
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = collection,
                    };
                    result = new Result<AdminModuleViewModel>(model);
                    ViewBag.ResultState = new { success = true, icon = "warning", message = "Şifrenin uzunluğu en az 8 karakter olmalıdır.!" };
                    return View(result);
                }
                var state = _service.Add(collection);

                if (state.IsSuccess)
                {
                    model = new AdminModuleViewModel()
                    {

                        AdminDo = collection,

                    };
                    result = new Result<AdminModuleViewModel>(model);
                    ViewBag.ResultState = new { success = true, icon = "success", message = state.Message };
                    return View(result);
                }
                else
                {
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = collection,

                    };
                    result = new Result<AdminModuleViewModel>(model);
                    ViewBag.ResultState = new { success = true, icon = "warning", message = state.Message };
                    return View(result);
                }
            }
            catch (Exception ex)
            {
                result = new Result<AdminModuleViewModel>(false, ex.Message);
                return View(result);
            }
        }

        [CheckAuth]
        public ActionResult Edit(int id)
        {
            ViewBag.ResultState = null;
            int is_admin = 0;// 0 ise admin değil yani kendi email veya username'ni değiştiremez 1 ise admin tüm bilgilerini değiştirebilir
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>();
            AdminModuleViewModel model = new AdminModuleViewModel();
            try
            {
                string token = Request.Cookies["usertoken"] != null && !string.IsNullOrEmpty(Request.Cookies["usertoken"].ToString()) ? Request.Cookies["usertoken"].ToString() : null;
                if (token == null)
                {
                    return RedirectToAction("Logout");
                }
                var admin = _service.GetByToken(token);
                if (!admin.IsSuccess || admin.Data == null || admin.Data.Id < 1)
                {
                    return RedirectToAction("Logout");
                }

                var user = _service.GetById(id);
                if (user.IsSuccess && user.Data != null && user.Data.Id > 0)
                {
                    // sisteme giriş yapabilecek rolid'ler {1, 2, 3}
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = user.Data,
                        IsAdmin = is_admin
                    };
                    result = new Result<AdminModuleViewModel>(model);
                }
                else
                {
                    result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "İşlemi Gerçekleştirme Yetkiniz bulunmamaktadır.");
                    return View(result);
                }
            }
            catch (Exception ex)
            {
                result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "" + ex.Message);
            }

            return View(result);
        }
        [CheckAuth]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AdminDO collection)
        {
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>();
            AdminModuleViewModel model = new AdminModuleViewModel();

            int is_admin = 0;
            try
            {
                string token = Request.Cookies["usertoken"] != null && !string.IsNullOrEmpty(Request.Cookies["usertoken"].ToString()) ? Request.Cookies["usertoken"].ToString() : null;
                if (token == null)
                {
                    return RedirectToAction("Logout");
                }
                var admin = _service.GetByToken(token);
                if (!admin.IsSuccess || admin.Data == null || admin.Data.Id < 1)
                {
                    return RedirectToAction("Logout");
                }

                var user = _service.GetById(id);
                if (user.IsSuccess && user.Data != null && user.Data.Id > 0)
                {

                    // sisteme giriş yapabilecek rolid'ler {1, 2, 3


                    user.Data.Name = collection.Name;
                    user.Data.Surname = collection.Surname;

                    var state = _service.Update(user.Data);

                    if (state.IsSuccess && state.Data != null)
                    {
                        ViewBag.ResultState = new { success = true, icon = "success", message = state.Message };
                    }
                    else
                    {
                        ViewBag.ResultState = new { success = true, icon = "warning", message = state.Message };
                    }
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = collection,
                        IsAdmin = is_admin

                    };
                    result = new Result<AdminModuleViewModel>(model);
                    return View(result);
                }
                else
                {
                    result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "İşlemi Gerçekleştirme Yetkiniz bulunmamaktadır.");
                    ViewBag.ResultState = new { success = true, icon = "error", message = "İşlemi Gerçekleştirme Yetkiniz bulunmamaktadır." };
                    return View(result);
                }
            }
            catch (Exception ex)
            {
                result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "" + ex.Message);
                ViewBag.ResultState = new { success = true, icon = "error", message = ex.Message };

            }

            return View(result);
        }

        // GET: Admin/Delete/5
        [CheckAuth]
        [HttpPost]
        public ActionResult Delete(AdminDO data)
        {

            try
            {
                int id = data.Id;

                string token = Request.Cookies["usertoken"] != null && !string.IsNullOrEmpty(Request.Cookies["usertoken"].ToString()) ? Request.Cookies["usertoken"].ToString() : null;
                if (token == null)
                {
                    return RedirectToAction("Logout");
                }
                var admin = _service.GetByToken(token);
                if (!admin.IsSuccess || admin.Data == null || admin.Data.Id < 1)
                {
                    return RedirectToAction("Logout");
                }

                var user = _service.GetById(id);
                if (user.IsSuccess && user.Data != null && user.Data.Id > 0)
                {


                    if (user.Data.Id == admin.Data.Id && (user.Data.Password != admin.Data.Password || user.Data.Email != admin.Data.Email))
                    {
                        ViewData["message"] = "İşlemi Gerçekleştirme Yetkiniz bulunmamaktadır.";
                        return View("~/Views/Admin/Error.cshtml");
                    }
                    Result<AdminDO> state = new Result<AdminDO>();

                    state = _service.Delete(user.Data);

                    return Ok(state);

                }
                else
                {
                    ViewData["message"] = "İşlemi Gerçekleştirme Yetkiniz bulunmamaktadır.";
                    return View("~/Views/Admin/Error.cshtml");
                }
            }
            catch (Exception ex)
            {
                Result<AdminDO> result = new Result<AdminDO>(false, "" + ex.Message);
                return Ok(result);
            }


        }




        // ================== <Start> Şifre ve Hesab Aktifleştirme İşlemleri ==================
        [CheckAuth]
        [HttpGet]
        public ActionResult PasswordChange(int id)
        {
            ViewBag.ResultState = null;
            AdminModuleViewModel model = new AdminModuleViewModel();
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>();
            try
            {
                string token = Request.Cookies["usertoken"] != null && !string.IsNullOrEmpty(Request.Cookies["usertoken"].ToString()) ? Request.Cookies["usertoken"].ToString() : null;
                if (token == null)
                {
                    return RedirectToAction("Logout");
                }
                var admin = _service.GetByToken(token);
                if (!admin.IsSuccess || admin.Data == null || admin.Data.Id < 1)
                {
                    return RedirectToAction("Logout");
                }

                var user = _service.GetById(id);
                if (user.IsSuccess && user.Data != null && user.Data.Id > 0)
                {
                    model = new AdminModuleViewModel()
                    {
                        AdminDo = new AdminDO
                        {
                            Token = user.Data.Token,
                        }
                    };
                    result = new Result<AdminModuleViewModel>(true, ResultTypeEnum.Success, model, "Kullanıcı bilgileri getirildi.");
                    return View(result);
                }

                else
                {
                    result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, "İşlemi Gerçekleştirme Yetkiniz bulunmamaktadır.");
                    return View(result);
                }
            }
            catch (Exception ex)
            {
                result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, model, ex.Message);
                return View(result);
            }

        }
        [CheckAuth]
        [HttpPost]
        [Produces("application/json")]
        public ActionResult PasswordChange([FromBody] JObject jsondata)
        {
            try
            {
                string admin_token = Request.Cookies["usertoken"] != null && !string.IsNullOrEmpty(Request.Cookies["usertoken"].ToString()) ? Request.Cookies["usertoken"].ToString() : null;
                if (admin_token == null)
                {
                    return RedirectToAction("Logout");
                }
                var admin = _service.GetByToken(admin_token);
                if (!admin.IsSuccess || admin.Data == null || admin.Data.Id < 1)
                {
                    return RedirectToAction("Logout");
                }

                string token = jsondata["token"].ToString();
                string password = jsondata["password"].ToString();
                string newPassword = jsondata["newpassword"].ToString();
                string newRePassword = jsondata["newrepassword"].ToString();


                Result<AdminDO> user = _service.GetByToken(token);
                if (user.IsSuccess && user.Data != null && user.Data.Id > 0)
                {


                    if (password == user.Data.Password)
                    {
                        if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 8 || string.IsNullOrEmpty(newRePassword) || newRePassword.Length < 8)
                        {
                            var result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, "Yeni şifreniz uzunluğu en az 8 karakter olmalı");
                            return Ok(result);
                        }
                        else if (newPassword == newRePassword)
                        {
                            user.Data.Password = newPassword;
                            Result<AdminDO> update = _service.UpdatePassword(user.Data);
                            return Ok(update);
                        }
                        else
                        {
                            var result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, "Yeni şifreleriniz eşleşmemiştir.");
                            return Ok(result);
                        }
                    }
                    else
                    {
                        var result = new Result<AdminModuleViewModel>(false, ResultTypeEnum.Error, "Girmiş olduğunuz eski şifreniz şifreniz ile eşleşmemiştir.");
                        return Ok(result);
                    }

                }

                else
                {
                    ViewData["message"] = "İşlemi Gerçekleştirme Yetkiniz bulunmamaktadır.";
                    return View("~/Views/Admin/Error.cshtml");
                }
            }
            catch (Exception ex)
            {
                var result = new Result<AdminDO>(false, "" + ex.Message);
                return Ok(result);
            }
            return Ok();

        }


        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [Produces("application/json")]
        public ActionResult ForgotPassword([FromBody] JObject jsondata)
        {
            Result<AdminDO> result = new Result<AdminDO>(false, "");
            try
            {
                string email = jsondata["email"].ToString();
                result = _service.GetByEmailResetPassword(email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                result = new Result<AdminDO>(false, "Hata  ::: \n" + ex.Message);
                return Ok(result);
            }

        }
        public ActionResult ApprovePassword(string t) // şifremi unuttum da gönderilen maildeki onay buttona tıklandığd anda
        {
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>();
            string token = t;

            if (string.IsNullOrEmpty(token))
            {
                ViewData["message"] = "Şifre Resetleme İşlemei Tamamlanamadı.";
                return View("~/Views/Admin/Error.cshtml");
            }

            Result<AdminDO> adminDO = _service.GetByToken(token);
            Result<AdminDO> userupdate = new Result<AdminDO>();

            if (adminDO.IsSuccess && adminDO.Data != null && adminDO.Data.Id > 0 && !string.IsNullOrEmpty(adminDO.Data.TmpPassword))
            {
                return RedirectToAction("ResetPassword", new { token = adminDO.Data.Token });
            }
            else if (adminDO.IsSuccess && adminDO.Data != null && adminDO.Data.Id > 0 && string.IsNullOrEmpty(adminDO.Data.TmpPassword))
            {
                ViewData["message"] = "Şifre Resetleme İşlemei Daha Önce Yapıldı.";
                return View("~/Views/Admin/Error.cshtml");
            }
            else
            {
                ViewData["message"] = "Şifre Resetleme İşlemei Tamamlanamadı.";
                return View("~/Views/Admin/Error.cshtml");
            }
        }
        public ActionResult ResetPassword(string token)
        {
            Result<AdminModuleViewModel> result;
            AdminModuleViewModel model = new AdminModuleViewModel
            {
                AdminDo = new AdminDO
                {
                    Token = token,
                }
            };
            result = new Result<AdminModuleViewModel>(model);

            return View(result);
        }
        [HttpPost]
        [Produces("application/json")]
        public ActionResult ResetPassword([FromBody] JObject jsondata)
        {
            Result<AdminDO> result = new Result<AdminDO>();


            try
            {
                string token = jsondata["token"].ToString();
                string tmp_password = jsondata["tmp_password"].ToString();
                string password = jsondata["newpassword"].ToString();
                string password_confirm = jsondata["newrepassword"].ToString();


                Result<AdminDO> adminDO = _service.GetByToken(token);
                Result<AdminDO> userupdate = new Result<AdminDO>();


                if (adminDO.IsSuccess && adminDO.Data != null && adminDO.Data.Id > 0 && !string.IsNullOrEmpty(adminDO.Data.TmpPassword))
                {
                    if (adminDO.Data.TmpPassword == tmp_password)
                    {
                        if (password.Length < 8)
                        {
                            return Ok(new { success = false, message = "Şifre En Az 8 karakter olmalı" });
                        }
                        if (password != password_confirm)
                        {
                            return Ok(new { success = false, message = "Şifreler eşleşmedi" });
                        }

                        adminDO.Data.Password = password;
                        adminDO.Data.TmpPassword = null;
                        userupdate = _service.UpdatePassword(adminDO.Data);
                        if (userupdate.IsSuccess)
                        {
                            return Ok(new { success = true, message = "Şifre Değiştirme İşlemi Başarılı Bir Şekilde Tamamlandı" });
                        }
                        else
                        {
                            return Ok(new { success = false, message = "Bilgiler güncellenirken bir sorun oluştu.lütfen tekrar deneyiniz." });
                        }
                    }
                    else
                    {
                        return Ok(new { success = false, message = "Onay Kodunuz Doğru değil." });
                    }

                }
                else
                {
                    return Ok(new { success = false, message = " Böyle bir kullanıcı bulunmamaktadır." });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = " HATA ::: \n" + ex.Message });
            }

        }
        [HttpGet]
        public ActionResult AccountConfirmed(string t, string c)
        {
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>();
            string token = t, confirmCode = c;

            if (string.IsNullOrEmpty(token))
            {
                ViewData["message"] = "Hesap Aklifleştirme İşlemi Başarısız.";
                return View("~/Views/Admin/Error.cshtml");
            }

            Result<AdminDO> adminDO = _service.GetByToken(token);
            Result<AdminDO> userupdate = new Result<AdminDO>();

            if (adminDO.IsSuccess && adminDO.Data != null && adminDO.Data.Id > 0 && adminDO.Data.ConfirmCode == confirmCode && adminDO.Data.AccountConfirm != true)
            {
                adminDO.Data.AccountConfirm = true;

                var state = _service.Update(adminDO.Data);

                if (state.IsSuccess && state.Data != null)
                {
                    //ViewBag.accountConfirmed = new { success = true, icon = "success", message = "Tebrikler Hesabınız Aktifleştirildi." };
                    //return RedirectToAction("Index", "Home");

                    ViewData["message"] = $"Sayın {state.Data.Name} {state.Data.Surname} hesabınız başarılı bir şekilde aktifleştirildi.";
                    return View("~/Views/Admin/AccountConfirmSuccess.cshtml");
                }
                else
                {
                    ViewData["message"] = "Bu Hesap Zaten Aklifleştirilmiştir..";
                    return View("~/Views/Admin/Error.cshtml");
                }
            }
            else if (adminDO.IsSuccess && adminDO.Data != null && adminDO.Data.Id > 0 && adminDO.Data.ConfirmCode == confirmCode && adminDO.Data.AccountConfirm == true)
            {
                ViewData["message"] = "Bu Hesap Zaten Aklifleştirilmiştir..";
                return View("~/Views/Admin/Error.cshtml");
            }
            else
            {
                ViewData["message"] = "Doğrulama İşlemi Başarısız.";
                return View("~/Views/Admin/Error.cshtml");
            }
        }




        // ================== </End> Şifre ve Hesab Aktifleştirme İşlemleri ==================

        [CheckAuth]

        [CheckAuth]
        public ActionResult UnConfirmedUserList()//UnConfirmedUserList
        {
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>();
            result = new Result<AdminModuleViewModel>(true, "");
            return View(result);
        }

        [HttpPost]
        public IActionResult GetUnConfirmedUserList()
        {
            try
            {
                var token = Request.Cookies["usertoken"].ToString();

                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;// sayfada kaç tane kayıt gösterilecek
                int skip = start != null ? Convert.ToInt32(start) : 0; //sayfa numarası

                var result = _service.GetUnConfirmedListFilteredData(new { token, skip, pageSize, sortColumn, sortColumnDirection, searchValue });

                if (result.IsSuccess && result.Data != null)
                {
                    var jsonData = new { recordsFiltered = result.Data.RecordsFiltered, recordsTotal = result.Data.RecordsTotal, data = result.Data.Data };
                    return Ok(jsonData);

                }
                else
                {
                    var jsonData = new { recordsFiltered = 0, recordsTotal = 0, data = "" };
                    return Ok(jsonData);
                }
            }
            catch (Exception ex)
            {
                var jsonData = new { recordsFiltered = 0, recordsTotal = 0, data = "" };
                return Ok(jsonData);
            }
        }



    }
}
