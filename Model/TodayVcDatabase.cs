using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NICVC.Model
{
    public class TodayVcDatabase

    {
        private SQLiteConnection conn;
        public TodayVcDatabase()
        {
            conn = DatabaseHelper.GetConnection("TodayVc.db3");
            conn.CreateTable<TodayVc>();
        }
        public IEnumerable<TodayVc> GetTodayVc(String Querryhere)
        {
            var list = conn.Query<TodayVc>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<TodayVc> GetTodayVcByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<TodayVc>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddTodayVc(TodayVc service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteTodayVc()
        {
            var del = conn.Query<TodayVc>("delete from TodayVc");
            return "success";
        }

        public string InsertTodayvclis(string insertintotodayvc)
        {
            string query = $"insert into TodayVc ('DateofVC','Dept_Name','Important','LevelName'," +
                 $"'Org_Name','Purpose','RequestedBy','Startingtime','StudioID','Studio_Name','VCEndTime','VCStatus'," +
                 $"'VC_ID','VcCoordEmail','VcCoordIpPhone','VcCoordMobile','VcCoordName','VccoordLandline','hoststudio','mcuip'," +
                 $"'mcuname','participantsExt','webroom' ,'VcStartDateTime','VcEndDateTime') " +
                 $" values {insertintotodayvc}";
            conn.Query<TodayVc>(query);


            return "success";
        }

      
    }
}
