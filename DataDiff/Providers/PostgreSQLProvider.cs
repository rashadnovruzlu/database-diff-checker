using Dapper;
using DataDiff.Engine.Enums;
using DataDiff.Engine.Models;
using System.Data;

namespace DataDiff.Providers
{
    public class PostgreSQLProvider : ISqlProvider
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

        public PostgreSQLProvider(string database, string server, string userId, string password)
        {
            Database = database;
            Server = server;
            UserId = userId;
            Password = password;
        }

        private DataContext CreateConnection()
        {
            return new DataContext(SqlProviders.PostgreSQL, new DbSettings()
            {
                Database = Database,
                Server = Server,
                UserId = UserId,
                Password = Password
            });
        }

        public List<string> GetAllTables()
        {
            return DataContext.DbConnection.Query<string>(@$"SELECT table_name
                                                            FROM information_schema.tables
                                                            WHERE table_schema NOT IN ('information_schema', 'pg_catalog')
                                                            AND table_type = 'BASE TABLE'
                                                            ORDER BY table_schema, table_name;")
                                          .ToList();
        }

        public List<string> GetColumnsByTableName(string tableName)
        {
            return DataContext.DbConnection.Query<string>(@$"SELECT column_name
                                                            FROM information_schema.columns
                                                            WHERE table_name = '{tableName}';")
                                          .ToList();
        }

        public List<IDictionary<string, object>> FetchData(string tableName)
        {
            return DataContext.DbConnection.Query($"SELECT * FROM public.\"{tableName}\"")
                                          .Select(x => (IDictionary<string, object>)x)
                                          .ToList();
        }

        public List<IDictionary<string, object>> FetchDataByClause(string tableName, string clauses)
        {
            return DataContext.DbConnection.Query($"SELECT * FROM public.\"{tableName}\" WHERE {clauses}")
                                         .Select(x => (IDictionary<string, object>)x)
                                         .ToList();
        }

        public string BuildClause(string column, object obj)
        {
            if (obj is int || obj is float || obj is double || obj is decimal ||
               obj is long || obj is short || obj is byte || obj is uint ||
               obj is ulong || obj is ushort || obj is sbyte)
            {
                return $"\"{column}\"={obj}";
            }
            else if (obj is bool)
            {
                return $"\"{column}\"={obj}";
            }
            else
            {
                return $"\"{column}\"='{obj}'";
            }
        }

        public string BuildClause(SystemDatabaseTableColumn column, object value)
        {
            if (column.DataType == ColumnDataTypes.Number)
            {
                return $"\"{column.Name}\" = {value}";
            }
            else
            {
                return $"\"{column.Name}\" = '{value}'";
            }
        }
        public string ConvertColumnNameToProviderSyntax(string columnName)
        {
            return $"\"{columnName}\"";
        }

        public string ConvertTableNameToProviderSyntax(string tableName)
        {
            return $"public.\"{tableName}\"";
        }

    }
}
