using DataDiff.Engine.Enums;

namespace DataDiff.Engine.Models
{
    public class SystemDatabaseTable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DependentTableName { get; set; }
        public List<SystemDatabaseTableColumn> Columns { get; set; } = new List<SystemDatabaseTableColumn>();
    }
}


