using Core.ResultType;
using DataObject.WM;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBFirstLayer.Interfaces
{
    public interface IDBFLocationLog
    {
        Result<ChaufferLocationLogDO> GetChauffeurDirectionListForToday(int chaufferId);
    }
}
