using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    public class MultipointStateDatabase
    {
        private SQLiteConnection conn;
        public MultipointStateDatabase()
        {
            conn = DatabaseHelper.GetConnection("MultipointState.db3");
            conn.CreateTable<MultipointState>();
        }
        public IEnumerable<MultipointState> GetMultipointState(String Querryhere)
        {
            var list = conn.Query<MultipointState>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<MultipointState> GetMultipointStateByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<MultipointState>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddMultipointState(MultipointState service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteMultipointState()
        {
            var del = conn.Query<MultipointState>("delete from MultipointState");
            return "success";
        }  

        public string UpdateCustomqueryMultipointState(string query)
        {
            var upd = conn.Query<MultipointState>(query);
            return "success";
        }
        public string InsertMultipointStatelist(string insertintomultipointstatevc)
        {
            string query = $"insert into MultipointState ('DateofVC','Dept_Name','Important','LevelName'," +
                 $"'Org_Name','Purpose','RequestedBy','Startingtime','StudioID','Studio_Name','VCEndTime','VCStatus'," +
                 $"'VC_ID','VcCoordEmail','VcCoordIpPhone','VcCoordMobile','VcCoordName','VccoordLandline','hoststudio','mcuip'," +
                 $"'mcuname','participantsExt','webroom','VcStartDateTime','VcEndDateTime' ) " +
                 $" values {insertintomultipointstatevc}";
            conn.Query<MultipointState>(query);


            return "success";
        }

    }
}
