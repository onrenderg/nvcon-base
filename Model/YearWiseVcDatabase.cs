using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
   public class YearWiseVcDatabase
    {
        private SQLiteConnection conn;
        public YearWiseVcDatabase()
        {
            conn = DatabaseHelper.GetConnection("YearWiseVc.db3");
            conn.CreateTable<YearWiseVc>();
        }
        public IEnumerable<YearWiseVc> GetYearWiseVc(String Querryhere)
        {
            var list = conn.Query<YearWiseVc>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<YearWiseVc> GetYearWiseVcByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<YearWiseVc>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddYearWiseVc(YearWiseVc service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteYearWiseVc()
        {
            var del = conn.Query<StateMaster>("delete from YearWiseVc");
            return "success";
        }

    }
}
