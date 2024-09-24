using DataDiff.Engine.Enums;
using System.Text.Json;

namespace DataDiff.Engine.Models
{
    public class SystemDatabaseTableColumn
    {
        public SystemDatabaseTableColumn()
        {
            ColumnAttributes = new Dictionary<ColumnAttributeType, object>();
        }

        public string TableName { get; set; }
        public string RelatedTableName { get; set; }
        public string Name { get; set; }
        public ColumnDataTypes DataType { get; set; }
        public bool IsForeign
        {
            get
            {
                return ColumnAttributes.ContainsKey(ColumnAttributeType.IsForeign);
            }
        }
        public bool IsUnique
        {
            get
            {
                return ColumnAttributes.ContainsKey(ColumnAttributeType.IsUnique);
            }
        }
        public bool IsPrimaryKey
        {
            get
            {
                return ColumnAttributes.ContainsKey(ColumnAttributeType.IsPrimaryKey);
            }
        }

        public Dictionary<ColumnAttributeType, object> ColumnAttributes { get; set; }

    }
}


