using AutoMapper;
using DataAccessLayer.WMDbContext;
using DataObject.WM;

namespace BusinessLogicLayer
{
    public class MapperBL
    {
        public static void Initialize()
        {
            Mapper.Initialize(config =>
            {
                config.CreateMap<Admin, AdminDO>().ReverseMap();
            });
        }
    }
}
