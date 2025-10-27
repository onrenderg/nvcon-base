using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    public class CurrentMonthVcDatabase
    {
        private SQLiteConnection conn;
        public CurrentMonthVcDatabase()
        {
            conn = DatabaseHelper.GetConnection("CurrentMonthVc.db3");
            conn.CreateTable<CurrentMonthVc>();
        }
        public IEnumerable<CurrentMonthVc> GetCurrentMonthVc(String Querryhere)
        {
            var list = conn.Query<CurrentMonthVc>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<CurrentMonthVc> GetCurrentMonthVcByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<CurrentMonthVc>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddCurrentMonthVc(CurrentMonthVc service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteCurrentMonthVc()
        {
            var del = conn.Query<StateMaster>("delete from CurrentMonthVc");
            return "success";
        }

        public string InsertCurrentMonthvclist(string insertintoschedulevc)
        {
            string query = $"insert into CurrentMonthVc ('DateofVC','Dept_Name','Important','LevelName'," +
                 $"'Org_Name','Purpose','RequestedBy','Startingtime','StudioID','Studio_Name','VCEndTime','VCStatus'," +
                 $"'VC_ID','VcCoordEmail','VcCoordIpPhone','VcCoordMobile','VcCoordName','VccoordLandline','hoststudio','mcuip'," +
                 $"'mcuname','participantsExt','webroom' ) " +
                 $" values {insertintoschedulevc}";
            conn.Query<CurrentMonthVc>(query);

         
                                                              
                                                             


            return "success";
        }
    }
}
