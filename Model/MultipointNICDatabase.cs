using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    public class MultipointNICDatabase
    {
        private SQLiteConnection conn;
        public MultipointNICDatabase()
        {
            conn = DatabaseHelper.GetConnection("MultipointNIC.db3");
            conn.CreateTable<MultipointNIC>();
        }
        public IEnumerable<MultipointNIC> GetMultipointNIC(String Querryhere)
        {
            var list = conn.Query<MultipointNIC>(Querryhere);
            return list.ToList();
        }
        public IEnumerable<MultipointNIC> GetMultipointNICByParameter(String Querryhere, string[] arrayhere)
        {
            var list = conn.Query<MultipointNIC>(Querryhere, arrayhere);
            return list.ToList();
        }
        public string AddMultipointNIC(MultipointNIC service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteMultipointNIC()
        {
            var del = conn.Query<MultipointNIC>("delete from MultipointNIC");
            return "success";
        }


        public string UpdateCustomqueryMultipointNIC(string query)
        {
            var upd = conn.Query<MultipointNIC>(query);
            return "success";
        }
        public string InsertMultipointNIClist(string insertintoMultipointNICvc)
        {
            string query = $"insert into MultipointNIC ('DateofVC','Dept_Name','Important','LevelName'," +
                 $"'Org_Name','Purpose','RequestedBy','Startingtime','StudioID','Studio_Name','VCEndTime','VCStatus'," +
                 $"'VC_ID','VcCoordEmail','VcCoordIpPhone','VcCoordMobile','VcCoordName','VccoordLandline','hoststudio','mcuip'," +
                 $"'mcuname','participantsExt','webroom' ,'VcStartDateTime','VcEndDateTime') " +
                 $" values {insertintoMultipointNICvc}";
            conn.Query<MultipointNIC>(query);


            return "success";
        }

    }
}
