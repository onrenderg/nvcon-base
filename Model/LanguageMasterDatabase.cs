using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    class LanguageMasterDatabase
    {

        private SQLiteConnection conn;


        public LanguageMasterDatabase()
        {
            conn = DatabaseHelper.GetConnection("LanguageMaster.db3");
            conn.CreateTable<LanguageMaster>();
        }
        public IEnumerable<LanguageMaster> GetLanguageMaster(string Querryhere)
        {
            var list = conn.Query<LanguageMaster>(Querryhere);
            return list.ToList();
        }
        public string AddLanguageMaster(LanguageMaster service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteLanguageMaster()
        {
            var del = conn.Query<LanguageMaster>("delete from LanguageMaster");
            return "success";
        }
    }
}
