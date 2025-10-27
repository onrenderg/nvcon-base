using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    class StateMasterDatabase
    {
        private SQLiteConnection conn;

        public StateMasterDatabase()
        {
            conn = DatabaseHelper.GetConnection("StateMaster.db3");
            conn.CreateTable<StateMaster>();
        }
        public IEnumerable<StateMaster> GetStateMaster(string Querryhere)
        {
            var list = conn.Query<StateMaster>(Querryhere);
            return list.ToList();
        }
        public string AddStateMaster(StateMaster service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteStateMaster()
        {
            var del = conn.Query<StateMaster>("delete from StateMaster");
            return "success";
        }
    }
}
