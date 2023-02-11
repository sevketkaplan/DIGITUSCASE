using Core.ResultType;
using DataObject.WM;
using DBFirstLayer.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBFirstLayer.Methods
{
    public class DBFLocationLog : IDBFLocationLog
    {
        public Result<ChaufferLocationLogDO> GetChauffeurDirectionListForToday(int chaufferId)
        {
            Result<ChaufferLocationLogDO> result;
            NpgsqlCommand command;
            ChaufferLocationLogDO log = new ChaufferLocationLogDO();


            if (chaufferId < 1)
            {
                result = new Result<ChaufferLocationLogDO>(false, "Şoför tespit edilemedi.");
                return result;
            }
            try
            {
                using (NpgsqlConnection conn = Connection.GetConnection.Connect())
                {
                    conn.Open();
                    string query = $"select * from chauffer_location_log where chauffer_id = {chaufferId} and DATE(log_date) = CURRENT_DATE";
                    command = new NpgsqlCommand(query, conn);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                log = new ChaufferLocationLogDO
                                {
                                    Id = Int32.Parse(reader["id"].ToString()),
                                    ChaufferId = Convert.ToInt32(reader["chauffer_id"]?.ToString()),
                                    DirectionsList = reader["directions_list"]?.ToString() ?? null,
                                    LogDate = Convert.ToDateTime(reader["log_date"]?.ToString())
                                };
                            }
                        }
                        else
                        {
                            result = new Result<ChaufferLocationLogDO>(false, ResultTypeEnum.Error, "O tarihte herhangi bir log bulunmadı.");
                            return result;
                        }
                    }

                    command.Dispose();

                    result = new Result<ChaufferLocationLogDO>(true, ResultTypeEnum.Success, log, "İşlem başarılı.");
                }
            }
            catch (Exception ex)
            {
                result = new Result<ChaufferLocationLogDO>(false, ResultTypeEnum.Error, log, "BİR HATA İLE KARŞILAŞILDI : " + ex.Message, "BİR HATA İLE KARŞILAŞILDI : " + ex.Message);
            }
            return result;
        }
    }
}
