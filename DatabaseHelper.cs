using SQLite;
using System.IO;

namespace NICVC
{
    public static class DatabaseHelper
    {
        public static SQLiteConnection GetConnection(string databaseName = "NICVC.db3")
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, databaseName);
            return new SQLiteConnection(dbPath);
        }
    }
}
