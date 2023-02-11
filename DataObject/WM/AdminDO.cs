using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataObject.WM
{
    public class AdminDO
    {


        public int Id { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public DateTime? CreateAt { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool? AccountConfirm { get; set; }
        public string TmpPassword { get; set; }
        public string ConfirmCode { get; set; }
        public string Token { get; set; }
        public bool? IsOnline { get; set; }

    }
}
