using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NICVC.Model
{
    class SaveUserPreferencesDatabase
    {
        private SQLiteConnection conn;
        public SaveUserPreferencesDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SaveUserPreferencesDatabase: Starting constructor");
                conn = DatabaseHelper.GetConnection("SaveUserPreferences.db3");
                System.Diagnostics.Debug.WriteLine("SaveUserPreferencesDatabase: Got connection");
                conn.CreateTable<SaveUserPreferences>();
                System.Diagnostics.Debug.WriteLine("SaveUserPreferencesDatabase: Created table");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveUserPreferencesDatabase Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SaveUserPreferencesDatabase Stack: {ex.StackTrace}");
                throw; // Re-throw to see the original error
            }
        }
        public IEnumerable<SaveUserPreferences> GetSaveUserPreferences(String Querryhere)
        {
            var list = conn.Query<SaveUserPreferences>(Querryhere);
            return list.ToList();
        }
        public string AddSaveUserPreferences(SaveUserPreferences service)
        {
            conn.Insert(service);
            return "success";
        }
        public string DeleteSaveUserPreferences()
        {
            var del = conn.Query<SaveUserPreferences>("delete from SaveUserPreferences");
            return "success";
        }


        public string CustomSaveUserPreferences(string query)
        {
            conn.Query<SaveUserPreferences>(query);
            return "success";
        }

    }
}
