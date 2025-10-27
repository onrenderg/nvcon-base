using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NICVC.Model
{
    class AlertableDatabase
    {
        private SQLiteConnection conn;
        public AlertableDatabase()
        {
            // Use the new MAUI database helper
            conn = DatabaseHelper.GetConnection("AlertableDatabase.db3");
            conn.CreateTable<Alertable>();
        }
        public IEnumerable<Alertable> GetAlertable(String Querryhere)
        {
            var list = conn.Query<Alertable>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<Alertable> GetAlertableByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<Alertable>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddAlertable(Alertable service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteAlertable()
        {
            var del = conn.Query<Alertable>("delete from Alertable");
            return "success";
        }
        public string DeletequeryCustomAlertable(string query)
        {
            var del = conn.Query<Alertable>(query);
            return "success";
        }

        public string DeleteCustomAlertable(string vcid)
        {
            var del = conn.Query<Alertable>($"delete from Alertable where vc_id='{vcid}'");
            return "success";
        }

        public string UpdateCustomAlertable(string vcid, string NotificationStartDate)
        {
            var upd = conn.Query<Alertable>($"update Alertable set NotificationStartDate='{NotificationStartDate}' where vc_id='{vcid}'");
            return "success";
        }

        public string UpdateCustomqueryAlertable(string query)
        {
            var upd = conn.Query<Alertable>(query);
            return "success";
        }

    }
}
