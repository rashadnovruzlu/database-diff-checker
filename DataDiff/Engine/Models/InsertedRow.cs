using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDiff.Engine.Models
{
    public class InsertedRow
    {
        public int Id { get; set; }
        public int ReleaseId { get; set; }
        public SystemDatabaseTable TableId { get; set; }
        public List<ColumnValue> RowData { get; set; }
        public List<ForeignColumn> ForeignColumns { get; set; }
    }

    public class UpdatedRow
    {
        public int Id { get; set; }
        public int ReleaseId { get; set; }
        public SystemDatabaseTable TableId { get; set; }
        public List<ColumnValue> RowData { get; set; }
        public List<ForeignColumn> ForeignColumns { get; set; }
        public List<ColumnValue> Filters { get; set; }
        public List<ForeignColumn> ForeignColumnsFilters { get; set; }

    }

    public class DeletedRow
    {
        public int Id { get; set; }
        public int ReleaseId { get; set; }
        public SystemDatabaseTable TableId { get; set; }
        public List<ColumnValue> Filters { get; set; }
        public List<ForeignColumn> ForeignColumnsFilters { get; set; }
    }
}


