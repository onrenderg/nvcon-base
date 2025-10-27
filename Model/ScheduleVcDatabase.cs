using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    public class ScheduleVcDatabase
    {
        private SQLiteConnection conn;
        public ScheduleVcDatabase()
        {
            conn = DatabaseHelper.GetConnection("ScheduleVc.db3");
            conn.CreateTable<ScheduleVc>();
        }
        public IEnumerable<ScheduleVc> GetScheduleVc(String Querryhere)
        {
            var list = conn.Query<ScheduleVc>(Querryhere);
            return list.ToList();
        }
        //public IEnumerable<ScheduleVc> GetScheduleVcByParameter(String Querryhere, string[] arrayhere)
        //{
        //    var list = conn.Query<ScheduleVc>(Querryhere, arrayhere);
        //    return list.ToList();
        //}
        public string AddScheduleVc(ScheduleVc service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteScheduleVc()
        {
            var del = conn.Query<ScheduleVc>("delete from ScheduleVc");
            return "success";
        }
        public string InsertSchedulevclis(string insertintoschedulevc)
        {
            string query = $"insert into ScheduleVc ('DateofVC','Dept_Name','Important','LevelName'," +
                 $"'Org_Name','Purpose','RequestedBy','Startingtime','StudioID','Studio_Name','VCEndTime','VCStatus'," +
                 $"'VC_ID','VcCoordEmail','VcCoordIpPhone','VcCoordMobile','VcCoordName','VccoordLandline','hoststudio','mcuip'," +
                 $"'mcuname','participantsExt','webroom','VcStartDateTime','VcEndDateTime') " +
                 $" values {insertintoschedulevc}";
            conn.Query<ScheduleVc>(query);


            return "success";
        }
    }
}
