using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NICVC.Model
{
  public  class StudioMasterDatabase
    {

        private SQLiteConnection conn;

        public StudioMasterDatabase()
        {
            conn = DatabaseHelper.GetConnection("StudioMaster.db3");
            conn.CreateTable<StudioMaster>();
        }
        public IEnumerable<StudioMaster> GetStudioMaster(string Querryhere)
        {
            var list = conn.Query<StudioMaster>(Querryhere);
            return list.ToList();
        }
        public string AddStudioMaster(StudioMaster service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteStudioMaster()
        {
            var del = conn.Query<StudioMaster>("delete from StudioMaster");
            return "success";
        }
    }
}
