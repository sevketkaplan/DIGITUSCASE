using Core.ResultType;
using DataObject.WM;
using DBFirstLayer.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBFirstLayer.Methods
{
    public class DBFUser : IDBFUser
    {
        public Result<AdminDO> GetByToken(string token)
        {
            Result<AdminDO> result;
            NpgsqlCommand command;
            AdminDO admin = new AdminDO();
            

            if (string.IsNullOrEmpty(token))
            {
                result = new Result<AdminDO>(false, "Token boş olamaz.");
                return result;
            }
            try
            {
                using (NpgsqlConnection conn = Connection.GetConnection.Connect())
                {
                    conn.Open();
                    string query = $"select * from admin where token = '{token}' and is_delete != true";
                    command = new NpgsqlCommand(query, conn);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                admin = new AdminDO
                                {
                                    Id = Int32.Parse(reader["id"].ToString()),
                                    Token = reader["token"].ToString(),
                                    Name = reader["name"].ToString(),
                                    Surname = reader["surname"].ToString(),
                                    Email = reader["email"].ToString(),
                                    RolId = Int32.Parse(reader["rol_id"].ToString()),
                                    CreateAt = DateTime.Parse(reader["create_at"].ToString()),
                                    UpdateAt = DateTime.Parse(reader["update_at"].ToString()),
                                    Phone = reader["phone"].ToString(),
                                    DeviceToken = reader["device_token"].ToString(),
                                };
                            }
                        }
                        else
                        {
                            result = new Result<AdminDO>(false, ResultTypeEnum.Error, "Bilinmeyen kullanıcı hatası.");
                            return result;
                        }
                    }

                    command.Dispose();                   

                    result = new Result<AdminDO>(true, ResultTypeEnum.Success, admin, "İşlem başarılı.");
                }
            }
            catch (Exception ex)
            {
                result = new Result<AdminDO>(false, ResultTypeEnum.Error, admin, "BİR HATA İLE KARŞILAŞILDI : " + ex.Message, "BİR HATA İLE KARŞILAŞILDI : " + ex.Message);
            }
            return result;
        }
    }
}
