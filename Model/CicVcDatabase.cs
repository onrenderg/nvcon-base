using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NICVC.Model
{
   public class CicVcDatabase
    {
        private SQLiteConnection conn;
        public CicVcDatabase()
        {
           conn = DatabaseHelper.GetConnection("CicVc.db3");
            conn.CreateTable<CicVc>();
        }
        public IEnumerable<CicVc> GetCicVc(String Querryhere)
        {
            var list = conn.Query<CicVc>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<CicVc> GetCicVcByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<CicVc>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddCicVc(CicVc service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteCicVc()
        {
            var del = conn.Query<CicVc>("delete from CicVc");
            return "success";
        }


        public string UpdateCustomqueryCicVc(string query)
        {
            var upd = conn.Query<CicVc>(query);
            return "success";
        }
        public string InsertCicVclist(string insertintoCicVcvc)
        {
            string query = $"insert into CicVc ('DateofVC','Dept_Name','Important','LevelName'," +
                 $"'Org_Name','Purpose','RequestedBy','Startingtime','StudioID','Studio_Name','VCEndTime','VCStatus'," +
                 $"'VC_ID','VcCoordEmail','VcCoordIpPhone','VcCoordMobile','VcCoordName','VccoordLandline','hoststudio','mcuip'," +
                 $"'mcuname','participantsExt','webroom' ,'VcStartDateTime','VcEndDateTime') " +
                 $" values {insertintoCicVcvc}";
            conn.Query<CicVc>(query);


            return "success";
        }

    }
}
