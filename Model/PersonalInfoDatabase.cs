using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    public class PersonalInfoDatabase
    {
        private SQLiteConnection conn;
        public PersonalInfoDatabase()
        {
            conn = DatabaseHelper.GetConnection("PersonalInfo.db3");
            conn.CreateTable<PersonalInfo>();
        }
        public IEnumerable<PersonalInfo> GetPersonalInfo(String Querryhere)
        {
            var list = conn.Query<PersonalInfo>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<PersonalInfo> GetPersonalInfoByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<PersonalInfo>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddPersonalInfo(PersonalInfo service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeletePersonalInfo()
        {
            var del = conn.Query<PersonalInfo>("delete from PersonalInfo");
            return "success";
        }
        public string customquery(string query)
        {
            var del = conn.Query<PersonalInfo>(query);
            return "success";
        }
    }
}
