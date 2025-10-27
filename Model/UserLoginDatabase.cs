using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace NICVC.Model
{

   public class UserLoginDatabase
    {
        private SQLiteConnection conn;
        public UserLoginDatabase()
        {
            conn = DatabaseHelper.GetConnection("UserLogin.db3");
            conn.CreateTable<UserLogin>();
        }
        public IEnumerable<UserLogin> GetUserLogin(string Querryhere)
        {
            var list = conn.Query<UserLogin>(Querryhere);
            return list.ToList();
        }
        public string AddUserLogin(UserLogin service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteUserLogin()
        {
            var del = conn.Query<UserLogin>("delete from UserLogin");
            return "success";
        }
        public IEnumerable<UserLogin> GetAllUserLogin()
        {
            var table = conn.Table<UserLogin>();
            return table.ToList();
        }
    }
}