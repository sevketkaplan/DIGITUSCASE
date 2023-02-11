using AutoMapper;
using Core.ResultType;
using Core.Security.Hashing;
using DataAccessLayer.WMDbContext;
using DataObject.Helpers;
using DataObject.ViewModels;
using DataObject.WM;
using Interfaces.Business;
using Interfaces.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Mail;
using Utility.Security.Jwt;


namespace BusinessLogicLayer
{

    public class AdminBL : IAdminBL
    {
        private IAdminService _adminService;
        private readonly AppSettings _appSettings;
        private ITokenHelper _tokenHelper;
        private IConfiguration _configurationBL;
        private IHostingEnvironment _env;

        public AdminBL(IOptions<AppSettings> appSettings, IAdminService adminService, ITokenHelper tokenHelper, IConfiguration configurationBL, IHostingEnvironment env)
        {
            _tokenHelper = tokenHelper;
            _appSettings = appSettings.Value;
            _adminService = adminService;
            _configurationBL = configurationBL;
            _env = env;
        }

        public Result<List<AdminDO>> GetAll()
        {
            Result<List<AdminDO>> result;
            List<AdminDO> recordList;
            try
            {
                var entityList = _adminService.GetAll();
                recordList = Mapper.Map<List<Admin>, List<AdminDO>>(entityList).ToList();
                result = new Result<List<AdminDO>>(true, ResultTypeEnum.Success, recordList, "İşlem Başarılı", "İşlem Başarılı");
            }
            catch (Exception ex)
            {
                recordList = new List<AdminDO>();
                result = new Result<List<AdminDO>>(false, ResultTypeEnum.Error, recordList, "İşlem Başarısız", "İşlem Başarısız.");
            }
            return result;
        }
        public Result<List<AdminDO>> GetAll(int usertype)
        {
            Result<List<AdminDO>> result;
            List<AdminDO> recordList;
            try
            {
                var entityList = _adminService.GetAsQueryable();
                recordList = Mapper.Map<List<Admin>, List<AdminDO>>(entityList).ToList();
                result = new Result<List<AdminDO>>(true, ResultTypeEnum.Success, recordList, "İşlem Başarılı", "İşlem Başarılı");
            }
            catch (Exception ex)
            {
                recordList = new List<AdminDO>();
                result = new Result<List<AdminDO>>(false, ResultTypeEnum.Error, recordList, "İşlem Başarısız", "İşlem Başarısız.");
            }
            return result;
        }

        public Result<AdminDO> GetByToken(string token)
        {
            Result<AdminDO> result;
            try
            {
                Admin admin = _adminService.GetAsQueryable(w => w.Token == token).FirstOrDefault();
                AdminDO adminDO = Mapper.Map<Admin, AdminDO>(admin);
                result = new Result<AdminDO>(adminDO);
            }
            catch (Exception ex)
            {

                result = new Result<AdminDO>(false, ResultTypeEnum.Error, "İşlem Başarısız");
            }
            return result;
        }
        public Result<List<AdminDO>> GetByEmail(string email)
        {
            Result<List<AdminDO>> result;
            List<AdminDO> recordList;
            try
            {
                var entityList = _adminService.GetAsQueryable(s => s.Email == email);
                recordList = Mapper.Map<List<Admin>, List<AdminDO>>(entityList).ToList();
                result = new Result<List<AdminDO>>(true, ResultTypeEnum.Success, recordList, "İşlem Başarılı");
            }
            catch (Exception ex)
            {
                recordList = new List<AdminDO>();
                result = new Result<List<AdminDO>>(false, ResultTypeEnum.Error, recordList, "İşlem Başarısız", "AdminBL.GetByEmail failed. ex.Message : " + ex.Message);
                result.SecondMessage = ex.Message;
            }
            return result;
        }
        public Result<AdminDO> GetById(int adminId)
        {
            Result<AdminDO> result;
            try
            {
                Admin admin = _adminService.GetAsQueryable(w => w.Id == adminId).FirstOrDefault();
                AdminDO adminDO = Mapper.Map<Admin, AdminDO>(admin);
                result = new Result<AdminDO>(adminDO);
            }
            catch (Exception ex)
            {

                result = new Result<AdminDO>(false, ResultTypeEnum.Error, "İşlem Başarısız");
            }
            return result;
        }

        public Result<AdminDO> Add(AdminDO adminDO)
        {
            Result<AdminDO> result;
            AdminDO user = new AdminDO();
            try
            {
                byte[] passwordHash, passwordSalt;
                HashingHelper.CreatePasswordHash(adminDO.Password, out passwordHash, out passwordSalt);


                adminDO.PasswordHash = passwordHash;
                adminDO.PasswordSalt = passwordSalt;
                adminDO.TmpPassword = null;
                adminDO.ConfirmCode = GenerateConfirmCode();

                user = adminDO;

                user.CreateAt = DateTime.Now;

                Result<AccessToken> accessToken = _tokenHelper.CreateToken(adminDO.Id);
                if (accessToken.IsSuccess)
                {
                    user.Token = accessToken.Data.Token;
                    Admin entity = Mapper.Map<AdminDO, Admin>(user);
                    if (_adminService.Exist(s => s.Email == adminDO.Email))
                    {
                        result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Mail Adresi Daha Önce Alınmıştır");
                    }
                    else
                    {
                        using (WMDbContext db = new WMDbContext())
                        {
                            using (var transaction = db.Database.BeginTransaction())
                            {
                                try
                                {

                                    db.Add(entity);
                                    db.SaveChanges();
                                    user.Id = entity.Id;
                                    result = new Result<AdminDO>(true, ResultTypeEnum.Success, user, "İşlem Başarılı");
                                    this.SendAccountConfiremdEmail(user);
                                    transaction.Commit();

                                }
                                catch (Exception dbex)
                                {
                                    transaction.Rollback();
                                    result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Kullanıcı oluşturulamadı.");

                                }
                            }
                        }

                    }
                }
                else
                {
                    result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Kullanıcı oluşturulamadı. Bir sorun oluştu lütfen yetkiliye bildiriniz");
                }
            }
            catch (Exception ex)
            {

                result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Kullanıcı kaydı sırasında bir sorun oluştu. Lütfen daha sonra tekrar deneyiniz.");
                result.SecondMessage = ex.Message;
            }
            return result;
        }

        public Result<AdminModuleViewModel> GetCounts()
        {
            Result<AdminModuleViewModel> result = new Result<AdminModuleViewModel>();
            AdminModuleViewModel data = new AdminModuleViewModel();
            int online = 0;
            int offline = 0;
            int confirmcont = 0;
            int nonconfirmcount = 0;
            try
            {
                online = _adminService.GetAsQueryable(s => s.IsOnline == true).Count();
                offline = _adminService.GetAsQueryable(s => s.IsOnline != true).Count();
                confirmcont = _adminService.GetAsQueryable(s => s.AccountConfirm == true).Count();
                nonconfirmcount = _adminService.GetAsQueryable(s => s.AccountConfirm != true).Count();
                result.IsSuccess = true;
                result.Message = "İşlem başarılı";
                data.OnlineCount = online;
                data.OfflineCount = offline;
                data.AccountConfirmCount = confirmcont;
                data.AccountNonConfirmCount = nonconfirmcount;
                result.Data = data;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "İşlem başarısız";
            }
            return result;

        }
        public Result<AdminDO> Update(AdminDO model)
        {
            Result<AdminDO> result = new Result<AdminDO>(false, "");
            try
            {
                var adminListByEmail = _adminService.GetAsQueryable(w => w.Email == model.Email && w.AccountConfirm == false);
                if (adminListByEmail != null && adminListByEmail.Count > 0)
                {
                    foreach (var item in adminListByEmail)
                    {
                        if (item.Id != model.Id)
                        {
                            return result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Mail Adresi Daha Önce Alınmıştır");
                        }
                    }
                }


                if (adminListByEmail != null && adminListByEmail.Count <= 1)
                {
                    var updateEntity = Mapper.Map<AdminDO, Admin>(model);
                    _adminService.Update(updateEntity);
                    return result = new Result<AdminDO>(true, ResultTypeEnum.Success, model, "İşlem Başarılı");
                }


            }
            catch (Exception ex)
            {
                result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Güncelleme sırasında bir hata meydana geldi lütfen bilgilerinizi kontrol edip tekrar deneyiniz");
                result.SecondMessage = ex.Message;
            }
            return result;
        }
        public Result<AdminDO> UpdatePassword(AdminDO model)
        {
            Result<AdminDO> result;
            try
            {
                byte[] passwordHash, passwordSalt;
                HashingHelper.CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);
                model.PasswordHash = passwordHash;
                model.PasswordSalt = passwordSalt;
                var updateEntity = Mapper.Map<AdminDO, Admin>(model);
                _adminService.Update(updateEntity);
                result = new Result<AdminDO>(true, ResultTypeEnum.Success, model, "İşlem Başarılı");

            }
            catch (Exception ex)
            {
                result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Güncelleme sırasında bir hata meydana geldi lütfen bilgilerinizi kontrol edip tekrar deneyiniz");
            }
            return result;
        }
        public Result<AdminDO> UpdateState(string token, bool state)
        {
            Result<AdminDO> result;
            try
            {
                Admin admin = _adminService.GetAsQueryable(s => s.Token == token).FirstOrDefault();
                admin.IsOnline = state;
                _adminService.Update(admin);
                AdminDO adminModel = Mapper.Map<Admin, AdminDO>(admin);
                result = new Result<AdminDO>(true, ResultTypeEnum.Success, adminModel, "İşlem Başarılı");

            }
            catch (Exception ex)
            {
                result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Güncelleme sırasında bir hata meydana geldi lütfen bilgilerinizi kontrol edip tekrar deneyiniz");
            }
            return result;
        }

        public Result<AdminDO> Delete(AdminDO model)
        {
            Result<AdminDO> result;
            try
            {
                var entity = Mapper.Map<AdminDO, Admin>(model);
                _adminService.Delete(entity);
                result = new Result<AdminDO>(true, ResultTypeEnum.Success, model, "İşlem Başarılı");
            }
            catch (Exception ex)
            {
                result = new Result<AdminDO>(false, ResultTypeEnum.Error, model, "İşlem Başarısız");
                result.SecondMessage = ex.Message;
            }
            return result;
        }
        public Result<AdminDO> Login(string email, string password)
        {
            Result<AdminDO> result = new Result<AdminDO>();
            string hataMesaji = "";
            if (email == "" || email == null || !email.Contains('@'))
            {
                hataMesaji += "* Lütfen Mail Adresinizi giriniz<br>";
            }
            if (password == "" || password == null || password.Length < 4)
            {
                hataMesaji += "* Lütfen Şifrenizi giriniz<br>";
            }

            if (hataMesaji != "")
            {
                result = new Result<AdminDO>(false, hataMesaji);
            }
            else
            {
                Admin admin = _adminService.GetAsQueryable(u => u.Email == email && u.Password == password).FirstOrDefault();
                AdminDO adminDo = Mapper.Map<Admin, AdminDO>(admin);
                if (adminDo == null)
                {
                    result = new Result<AdminDO>(false, "Giriş Bilgilerinizi Kontrol Ediniz");
                }
                else
                {
                    result = new Result<AdminDO>(ResultTypeEnum.Success, adminDo, "Giriş Başarılı", "");
                    //var cookieOptions = new CookieOptions()
                    //{
                    //    Path = "/",
                    //    HttpOnly = false,
                    //    Expires = DateTime.Now.AddMonths(1),
                    //};
                }

            }
            return result;
        }

        public Result<AdminDO> GetByEmailResetPassword(string email)
        {
            Result<AdminDO> result;
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    Admin admin = _adminService.GetAsQueryable(w => w.Email == email).FirstOrDefault();
                    if (admin != null && admin.AccountConfirm == true)
                    {
                        admin.TmpPassword = GeneratePassword();
                        AdminDO adminDO = Mapper.Map<Admin, AdminDO>(admin);
                        // Result<AdminDO> resetstate = this.UpdatePassword(adminDO);

                        var state = this.SendResetPasswordEmail(adminDO);
                        if (state.IsSuccess)
                        {
                            _adminService.Update(admin);
                        }
                        var tmp = new AdminDO
                        {
                            AccountConfirm = admin.AccountConfirm
                        };
                        result = new Result<AdminDO>(true, ResultTypeEnum.Success, tmp, "Şifre sıfırlama işlemi tamamlandı. Bilgileriniz mail adresinize gönderilmiştir");
                    }
                    else if (admin != null && admin.AccountConfirm != true)
                    {
                        var tmp = new AdminDO
                        {
                            AccountConfirm = admin.AccountConfirm
                        };
                        result = new Result<AdminDO>(false, ResultTypeEnum.Error, tmp, "Lütfen Önce Hesabı Aktifleştiriniz. Akifleştirmek İçin Hesabınıza Bir Onay Maili Yollanmıştı");
                    }
                    else
                    {
                        result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Girdiğiniz Email Sistemde Kayıtlı Değildir");
                    }
                }
                catch (Exception ex)
                {
                    result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Şifre sıfırlanırken beklenmedik kritik hata meydana geldi. Bilgileri kontrol edip tekrar deneyiniz");
                }
            }
            else
            {
                result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Email adresi boş bırakılamaz");

            }
            return result;
        }

        public Result<AdminDO> ExistByEmail(string email)
        {
            Result<AdminDO> result;
            try
            {
                Admin admin = _adminService.GetAsQueryable(w => w.Email == email).FirstOrDefault();
                AdminDO adminDO = Mapper.Map<Admin, AdminDO>(admin);
                result = new Result<AdminDO>(adminDO);
            }
            catch (Exception ex)
            {
                result = new Result<AdminDO>(false, ResultTypeEnum.Error, "İşlem Başarısız");
            }
            return result;
        }


        private string GeneratePassword(int length = 8)
        {
            //const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            //StringBuilder res = new StringBuilder();
            //Random rnd = new Random();
            //while (0 < length--)
            //{
            //    res.Append(valid[rnd.Next(valid.Length)]);
            //}
            //return res.ToString();

            return "" + new Random().Next(100000, 999999);
        }
        private string GenerateConfirmCode(int length = 8)
        {
            //const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            //StringBuilder res = new StringBuilder();
            //Random rnd = new Random();
            //while (0 < length--)
            //{
            //    res.Append(valid[rnd.Next(valid.Length)]);
            //}
            //return res.ToString();

            return "" + new Random().Next(100000, 999999);
        }

        // =========== Kullanıcı Oluşturulurken Artık Doğrulama Email göndrilmeyecek
        public Result<bool> SendAccountConfiremdEmail(AdminDO adminDO)
        {
            bool state = false;
            string base_link = _configurationBL.GetSection("webDomain").GetSection("domain").Value;
            string redirect_url = base_link + "/Admin/AccountConfirmed?t=" + adminDO.Token + "&c=" + adminDO.ConfirmCode;
            string _target = "_blank";
            string contact_url = "https://localhost:5001/";
            string is_delete = "";



            string path = $"/mail/account_confirm_tr.html";
            try
            {
                string FilePath = _env.WebRootPath + path;
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();
                string name_surname = adminDO.Name.Trim() + " " + adminDO.Surname.Trim();

                MailText = MailText.Replace("LOGO_URL", "");
                MailText = MailText.Replace("NAME_SURNAME", name_surname);
                MailText = MailText.Replace("ACCOUNT_CONFIRM_CODE", adminDO.ConfirmCode.Trim());
                MailText = MailText.Replace("IS_DELETE", is_delete);
                MailText = MailText.Replace("REDIRECT_URL", redirect_url);
                MailText = MailText.Replace("IS_BLANCK", _target);
                MailText = MailText.Replace("CONTACT_URL", contact_url);

                string email_address = _configurationBL.GetSection("EmailSettings").GetSection("email_address").Value;
                string email_password = _configurationBL.GetSection("EmailSettings").GetSection("email_password").Value;
                string email_name = _configurationBL.GetSection("EmailSettings").GetSection("email_name").Value;
                string email_title = "Hesap Doğrulama.";

                string email_host = _configurationBL.GetSection("EmailSettings").GetSection("email_host").Value;
                string email_port = _configurationBL.GetSection("EmailSettings").GetSection("email_port").Value;
                MailMessage meailit = new MailMessage();
                meailit.To.Add(new MailAddress(adminDO.Email, email_name));
                meailit.From = new MailAddress(email_address, email_name);

                meailit.Subject = email_title;
                meailit.Body = MailText;

                meailit.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();

                client.Host = email_host;
                client.Port = Int32.Parse(email_port);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(email_address, email_password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;

                client.Send(meailit);
                state = true;
            }
            catch (Exception ex)
            {
                throw;
            }
            return new Result<bool>(state);
        }
        public Result<bool> SendResetPasswordEmail(AdminDO adminDO)
        {
            bool state = false;

            string base_link = _configurationBL.GetSection("webDomain").GetSection("domain").Value;
            string redirect_url = base_link + "/Admin/ApprovePassword?t=" + adminDO.Token;
            string _target = "_blank";
            string contact_url = "https://localhost:5001/";
            string is_delete = "";



            string path = $"/mail/password_forgot_tr.html";
            try
            {
                string FilePath = _env.WebRootPath + path;
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();

                string name_surname = adminDO.Name.Trim() + " " + adminDO.Surname.Trim();

                MailText = MailText.Replace("NAME_SURNAME", name_surname);
                MailText = MailText.Replace("RESET_PASSWORD_CONFIRM_CODE", adminDO.TmpPassword);
                MailText = MailText.Replace("IS_DELETE", is_delete);
                MailText = MailText.Replace("REDIRECT_URL", redirect_url);
                MailText = MailText.Replace("IS_BLANCK", _target);
                MailText = MailText.Replace("CONTACT_URL", contact_url);


                string email_address = _configurationBL.GetSection("EmailSettings").GetSection("email_address").Value;
                string email_password = _configurationBL.GetSection("EmailSettings").GetSection("email_password").Value;
                string email_name = _configurationBL.GetSection("EmailSettings").GetSection("name").Value;
                string email_title = "Şifremi Unuttum.";
                string email_host = _configurationBL.GetSection("EmailSettings").GetSection("email_host").Value;
                string email_port = _configurationBL.GetSection("EmailSettings").GetSection("email_port").Value;
                MailMessage meailit = new MailMessage();
                meailit.To.Add(new MailAddress(adminDO.Email, email_name));
                meailit.From = new MailAddress(email_address, email_name);

                meailit.Subject = email_title;
                meailit.Body = MailText;

                meailit.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();

                client.Host = email_host;
                client.Port = Int32.Parse(email_port);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(email_address, email_password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;

                client.Send(meailit);
                state = true;
            }
            catch (Exception ex)
            {
                throw;
            }

            return new Result<bool>(state);
        }
        public Result<DataTable> GetUnConfirmedListFilteredData(dynamic dynamic)
        {
            Result<DataTable> result = null; ;
            DataTable dataTable;
            try
            {
                string token = dynamic.GetType().GetProperty("token").GetValue(dynamic);

                int skip = dynamic.GetType().GetProperty("skip").GetValue(dynamic);
                int pageSize = dynamic.GetType().GetProperty("pageSize").GetValue(dynamic);
                string sortColumn = dynamic.GetType().GetProperty("sortColumn").GetValue(dynamic);
                string sortColumnDirection = dynamic.GetType().GetProperty("sortColumnDirection").GetValue(dynamic);
                string searchValue = dynamic.GetType().GetProperty("searchValue").GetValue(dynamic);


                using (WMDbContext db = new WMDbContext())
                {
                    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                    var manager = db.Admin.FirstOrDefault(i => i.Token == token);



                    IQueryable<AdminDO> data = from admin in db.Admin
                                               where admin.AccountConfirm != true
                                               select new AdminDO
                                               {
                                                   Id = admin.Id,
                                                   Password = admin.Password,
                                                   Name = admin.Name,
                                                   Surname = admin.Surname,
                                                   Email = admin.Email,
                                                   CreateAt = admin.CreateAt,


                                               };

                    var all_data_count = data.Count();
                    // burada datatabledki arama inputten gelen value hangi kolönlerde arama işlemi yapılacak diye belirtiyoruz. veritabanda kolön tipleri farklı olabileceğinden arama öncesi tüm kolönler char tipne dönüştürmede fayda var.
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        data = data.Where(i => (!string.IsNullOrEmpty(i.Name) ? (i.Name.Contains(searchValue) || i.Name.ToUpper().Contains(searchValue.ToUpper()) || i.Name.ToLower().Contains(searchValue.ToLower())) : true)
                                                || (!string.IsNullOrEmpty(i.Surname) ? (i.Surname.Contains(searchValue) || i.Surname.ToUpper().Contains(searchValue.ToUpper()) || i.Surname.ToLower().Contains(searchValue.ToLower())) : true)
                                                || (!string.IsNullOrEmpty(i.Email) ? i.Email.Contains(searchValue) : true)
                                         );


                    }
                    var filteredDataCount = data.Count();

                    // =======================  Bu Kod kapalı =======================
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        data = data.OrderBy(sortColumn + " " + sortColumnDirection);
                    }

                    if (pageSize != -1) // eğer kulanıcı tüm verileri göster seçmiş ise limit işlemi iptal olsun.
                    {
                        data = data.Skip(skip).Take(pageSize);
                    }

                    var res = data.ToList();

                    // =======================  Bu Kod kapalı =======================                    

                    //var userList = Mapper.Map<List<Admin>, List<AdminDO>>(res);
                    var userList = res;

                    dataTable = new DataTable
                    {
                        RecordsFiltered = !string.IsNullOrEmpty(searchValue) ? filteredDataCount : all_data_count,
                        RecordsTotal = all_data_count,
                        Data = userList
                    };

                    result = new Result<DataTable>(true, ResultTypeEnum.Success, dataTable, "");
                }


            }
            catch (Exception ex)
            {
                result = new Result<DataTable>(false, ResultTypeEnum.Error, "An error occured when getting Admin Modules ! Ex : ", ex.Message);
            }
            return result;


        }


    }


}
