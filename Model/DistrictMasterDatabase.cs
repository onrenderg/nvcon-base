using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    public class DistrictMasterDatabase
    {
        private SQLiteConnection conn;
        public DistrictMasterDatabase()
        {
            conn = DatabaseHelper.GetConnection("DistrictMaster.db3");
            conn.CreateTable<DistrictMaster>();
        }
        public IEnumerable<DistrictMaster> GetDistrictMaster(String Querryhere)
        {
            var list = conn.Query<DistrictMaster>(Querryhere);
            return list.ToList();
        }
        public string AddDistrictMaster(DistrictMaster service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteDistrictMaster()
        {
            var del = conn.Query<StateMaster>("delete from DistrictMaster");
            return "success";
        }
    }
}