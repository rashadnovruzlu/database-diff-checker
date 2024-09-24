using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDiff.Engine.Params
{
    public class InsertQueryResult
    {
        public List<string> Queries { get; set; }
        public class UnrelatedRow
        {
            public string TableName { get; set; }
            public string ColumnName { get; set; }
            public object Value { get; set; }

            public UnrelatedRow(string tableName, string columnName, object value)
            {
                TableName = tableName;
                ColumnName = columnName;
                Value = value;
            }
        }
        public List<UnrelatedRow> UnrelatedRows { get; set; }
        public InsertQueryResult(List<string> queries, List<UnrelatedRow> unrelatedRows)
        {
            Queries = queries;
            UnrelatedRows = unrelatedRows;
        }
    }
}
