using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NICVC.Model
{
    class ImportantVcDatabase
    {
        private SQLiteConnection conn;
        public ImportantVcDatabase()
        {
            conn = DatabaseHelper.GetConnection("ImportantVc.db3");
            conn.CreateTable<ImportantVc>();
        }
        public IEnumerable<ImportantVc> GetImportantVc(String Querryhere)
        {
            var list = conn.Query<ImportantVc>(Querryhere);
            return list.ToList();
        }
      
        public string AddImportantVc(ImportantVc service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteImportantVc()
        {
            var del = conn.Query<StateMaster>("delete from ImportantVc");
            return "success";
        }
    }
}
