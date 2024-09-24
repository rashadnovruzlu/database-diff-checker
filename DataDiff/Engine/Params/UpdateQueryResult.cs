using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDiff.Engine.Params
{
    public class UpdateQueryResult
    {
        public class UpdateQueryColumnValue
        {
            public object SourceColumnValue { get; set; }
            public object DestinationColumnValue { get; set; }

            public UpdateQueryColumnValue(object sourceColumnValue, object destinationColumnValue)
            {
                SourceColumnValue = sourceColumnValue;
                DestinationColumnValue = destinationColumnValue;
            }
        }
        public class UpdateQuery
        {
            public string Query { get; set; }
            public Dictionary<string, UpdateQueryColumnValue> DifferentColumns { get; set; }
            public UpdateQuery(string query, Dictionary<string, UpdateQueryColumnValue> differentColumns)
            {
                Query = query;
                DifferentColumns = differentColumns;
            }
        }
        public List<UpdateQuery> Queries { get; set; }
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
        public UpdateQueryResult(List<UpdateQuery> queries, List<UnrelatedRow> unrelatedRows)
        {
            Queries = queries;
            UnrelatedRows = unrelatedRows;
        }
    }
}
