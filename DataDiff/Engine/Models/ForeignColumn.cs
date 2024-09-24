namespace DataDiff.Engine.Models
{
    public class ForeignColumn
    {
        public SystemDatabaseTable TableId { get; set; }
        public string ForeignColumnName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
    }
}


