using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
   public class LatLongDatabase
    {
        private SQLiteConnection conn;
        public LatLongDatabase()
        {
            conn = DatabaseHelper.GetConnection("LatLong.db3");
            conn.CreateTable<LatLong>();
        }
        public IEnumerable<LatLong> GetLatLong(String Querryhere)
        {
            var list = conn.Query<LatLong>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<LatLong> GetLatLongByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<LatLong>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddLatLong(LatLong service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteLatLong()
        {
            var del = conn.Query<StateMaster>("delete from LatLong");
            return "success";
        }

    }
}
