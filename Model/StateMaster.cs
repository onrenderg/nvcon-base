using SQLite;

namespace NICVC.Model
{
    public class StateMaster
    {
        [PrimaryKey]
        public string StateID { get; set; }
        public string StateName { get; set; }
    }
}