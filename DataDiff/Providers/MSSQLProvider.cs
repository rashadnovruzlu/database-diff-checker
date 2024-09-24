using Dapper;
using DataDiff.Engine;
using DataDiff.Engine.Enums;
using DataDiff.Engine.Models;
using System.Data;

namespace DataDiff.Providers
{
    public class MSSQLProvider : ISqlProvider
    {
        public readonly string Database;
        public readonly string Server;
        public readonly string UserId;
        public readonly string Password;

        private DataContext _dataContext;

        public DataContext DataContext
        {
            get
            {
                if (_dataContext == null)
                {
                    _dataContext = CreateConnection();
                }

                return _dataContext!;
            }
        }

        public MSSQLProvider(string database, string server, string userId, string password)
        {
            Database = database;
            Server = server;
            UserId = userId;
            Password = password;
        }

        private DataContext CreateConnection()
        {
            return new DataContext(SqlProviders.MSSQL, new DbSettings()
            {
                Database = Database,
                Server = Server,
                UserId = UserId,
                Password = Password
            });
        }

        public List<IDictionary<string, object>> FetchData(string tableName)
        {
            return DataContext.DbConnection.Query($"SELECT * FROM [dbo].{tableName}")
                                          .Select(x => (IDictionary<string, object>)x)
                                          .ToList();
        }

        public List<IDictionary<string, object>> FetchDataByClause(string tableName, string clauses)
        {
            return DataContext.DbConnection.Query($"SELECT * FROM [dbo].{tableName} WHERE {clauses}")
                                         .Select(x => (IDictionary<string, object>)x)
                                         .ToList();
        }

        public string BuildClause(string column, object obj)
        {
            if (obj is int || obj is float || obj is double || obj is decimal ||
               obj is long || obj is short || obj is byte || obj is uint ||
               obj is ulong || obj is ushort || obj is sbyte)
            {
                return $"{column}={obj}";
            }
            else if (obj is bool)
            {
                return $"{column}={obj}";
            }
            else
            {
                return $"{column}='{obj}'";
            }
        }

        public string BuildClause(SystemDatabaseTableColumn column, object value)
        {
            if (column.DataType == ColumnDataTypes.Number)
            {
                return $"{column.Name} = {value}";
            }
            else
            {
                return $"{column.Name} = '{value}'";
            }
        }

        public string ConvertColumnNameToProviderSyntax(string columnName)
        {
            return $"{columnName}";
        }
        public string ConvertTableNameToProviderSyntax(string tableName)
        {
            return $"\"[dbo].{tableName}\"";
        }

        public List<string> GetAllTables()
        {
            throw new NotImplementedException();
        }

        public List<string> GetColumnsByTableName(string tableName)
        {
            throw new NotImplementedException();
        }
    }
}
