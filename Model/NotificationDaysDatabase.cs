using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
   public class NotificationDaysDatabase
    {
        private SQLiteConnection conn;
        public NotificationDaysDatabase()
        {
            conn = DatabaseHelper.GetConnection("NotificationDays.db3");
            conn.CreateTable<NotificationDays>();
        }
        public IEnumerable<NotificationDays> GetNotificationDays(string Querryhere)
        {
            var list = conn.Query<NotificationDays>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<NotificationDays> GetNotificationDaysByParameter(string Querryhere, string[] arrayhere)
        {
            var list = conn.Query<NotificationDays>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddNotificationDays(NotificationDays service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteNotificationDays()
        {
            var del = conn.Query<StateMaster>("delete from NotificationDays");
            return "success";
        }

    }
}
