using DataDiff.Engine.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDiff.Providers
{
    public interface ISqlProvider
    {
        DataContext DataContext { get; }
        List<IDictionary<string, object>> FetchDataByClause(string tableName, string clauses);
        List<IDictionary<string, object>> FetchData(string tableName);
        string BuildClause(string column, object obj);
        string BuildClause(SystemDatabaseTableColumn column, object value);
        string ConvertColumnNameToProviderSyntax(string columnName);
        string ConvertTableNameToProviderSyntax(string tableName);
        List<string> GetAllTables();
        List<string> GetColumnsByTableName(string tableName);
    }
}
