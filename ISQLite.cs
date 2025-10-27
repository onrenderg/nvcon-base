using System;
namespace NICVC
{
    public interface ISQLite
    {
        SQLite.SQLiteConnection GetConnection();
    }
}
