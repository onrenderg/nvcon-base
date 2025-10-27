using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
   public class DashboardDatabase
    {
        private SQLiteConnection conn;
        public DashboardDatabase()
        {
            conn = DatabaseHelper.GetConnection("Dashboard.db3");
            conn.CreateTable<Dashboard>();
        }
        public IEnumerable<Dashboard> GetDashboard(String Querryhere)
        {
            var list = conn.Query<Dashboard>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<Dashboard> GetDashboardByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<Dashboard>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddDashboard(Dashboard service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteDashboard()
        {
            var del = conn.Query<StateMaster>("delete from Dashboard");
            return "success";
        }

    }
}
