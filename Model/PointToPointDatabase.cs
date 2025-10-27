using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    public class PointToPointDatabase
    {
        private SQLiteConnection conn;
        public PointToPointDatabase()
        {
            conn = DatabaseHelper.GetConnection("PointToPoint.db3");
            conn.CreateTable<PointToPoint>();
        }
        public IEnumerable<PointToPoint> GetPointToPoint(String Querryhere)
        {
            var list = conn.Query<PointToPoint>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<PointToPoint> GetPointToPointByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<PointToPoint>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddPointToPoint(PointToPoint service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeletePointToPoint()
        {
            var del = conn.Query<PointToPoint>("delete from PointToPoint");
            return "success";
        }


        public string UpdateCustomqueryPointToPoint(string query)
        {
            var upd = conn.Query<PointToPoint>(query);
            return "success";
        }
        public string InsertPointToPointlist(string insertintoPointToPointvc)
        {
            string query = $"insert into PointToPoint ('DateofVC','Dept_Name','Important','LevelName'," +
                 $"'Org_Name','Purpose','RequestedBy','Startingtime','StudioID','Studio_Name','VCEndTime','VCStatus'," +
                 $"'VC_ID','VcCoordEmail','VcCoordIpPhone','VcCoordMobile','VcCoordName','VccoordLandline','hoststudio','mcuip'," +
                 $"'mcuname','participantsExt','webroom','VcStartDateTime','VcEndDateTime' ) " +
                 $" values {insertintoPointToPointvc}";
            conn.Query<PointToPoint>(query);


            return "success";
        }

    }
}
