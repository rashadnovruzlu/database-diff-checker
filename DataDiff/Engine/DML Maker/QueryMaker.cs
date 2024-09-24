using Dapper;
using DataDiff;
using DataDiff.Engine.Enums;
using DataDiff.Engine.Models;
using DataDiff.Engine.Settings;
using DataDiff.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DataDiff.Engine.DML_Maker
{
    public class QueryMakerParameter
    {
        public readonly ISqlProvider SourceDatabase;
        public readonly ISqlProvider DestinationDatabase;
        public readonly ITableLoader TableLoader;

        public readonly string TableName;

        public QueryMakerParameter(ITableLoader tableLoader, ISqlProvider sourceDatabase, ISqlProvider destinationDatabase, string tableName)
        {
            TableLoader = tableLoader;
            SourceDatabase = sourceDatabase;
            DestinationDatabase = destinationDatabase;
            TableName = tableName;
        }

    }

    public abstract class QueryMaker
    {
        protected readonly string TableName;

        protected SystemDatabaseTable Table { get { return TableLoader.Tables.Single(x => x.Name == TableName); } }

        protected readonly ISqlProvider SourceDatabase;

        protected readonly ISqlProvider DestinationDatabase;

        public readonly ITableLoader TableLoader;

        public QueryMaker(QueryMakerParameter queryMakerParameter)
        {
            TableName = queryMakerParameter.TableName;

            SourceDatabase = queryMakerParameter.SourceDatabase;

            DestinationDatabase = queryMakerParameter.DestinationDatabase;

            TableLoader = queryMakerParameter.TableLoader;
        }


        public object FindSameRowIdOnDestination(string tableName, object id)
        {
            var table = TableLoader.Tables.Single(x => x.Name == tableName);

            var primaryKeyColumn = table.Columns.Single(x => x.IsPrimaryKey);

            var rowInSource = SourceDatabase.FetchDataByClause(tableName, $"{SourceDatabase.BuildClause(primaryKeyColumn, id)}").Single();

            var columnFilter = new Dictionary<SystemDatabaseTableColumn, object>();

            if (table.Columns.Any(column => column.IsUnique && !column.IsForeign) && !table.Columns.Any(column => column.IsUnique && column.IsForeign))
            {
                var clauses = string.Join(" AND ",
                              table.Columns
                             .Where(column => column.IsUnique && !column.IsForeign)
                             .Select(x => $"{DestinationDatabase.BuildClause(x, rowInSource[x.Name])}"));

                var rowInDestination = DestinationDatabase.FetchDataByClause(tableName, clauses).SingleOrDefault();

                return rowInDestination?["Id"];
            }
            else if (table.Columns.Any(column => column.IsUnique && column.IsForeign))
            {
                foreach (var item in table.Columns.Where(column => column.IsUnique && column.IsForeign))
                {
                    var refId = FindSameRowIdOnDestination(item.RelatedTableName.ToString(), rowInSource[item.Name]);

                    if (refId != null)
                    {
                        columnFilter.Add(item, id);
                    }
                }

                var uniqueColumns = Table.Columns.Where(column => column.IsUnique && !column.IsForeign).ToList();

                foreach (var item in uniqueColumns)
                {
                    columnFilter.Add(item, rowInSource[item.Name]);
                }

                var clauses = string.Join(" AND ", uniqueColumns.Select(x => $"{DestinationDatabase.BuildClause(x, rowInSource[x.Name])}"));

                var equivalentRow = DestinationDatabase.FetchDataByClause(tableName, clauses).SingleOrDefault();

                return equivalentRow?["Id"];
            }

            return null;
        }

        public object FindSameRowIdOnSource(string tableName, object id)
        {
            var table = TableLoader.Tables.Single(x => x.Name == tableName);

            var primaryKeyColumn = table.Columns.Single(x => x.IsPrimaryKey);

            var rowInSource = DestinationDatabase.FetchDataByClause(tableName, $"{DestinationDatabase.BuildClause(primaryKeyColumn, id)}").Single();

            var columnFilter = new Dictionary<SystemDatabaseTableColumn, object>();

            if (table.Columns.Any(column => column.IsUnique && !column.IsForeign) && !table.Columns.Any(column => column.IsUnique && column.IsForeign))
            {
                var clauses = string.Join(" AND ",
                              table.Columns
                             .Where(column => column.IsUnique && !column.IsForeign)
                             .Select(x => $"{SourceDatabase.BuildClause(x, rowInSource[x.Name])}"));

                var rowInDestination = SourceDatabase.FetchDataByClause(tableName, clauses).SingleOrDefault();

                return rowInDestination?["Id"];
            }
            else if (table.Columns.Any(column => column.IsUnique && column.IsForeign))
            {
                foreach (var item in table.Columns.Where(column => column.IsUnique && column.IsForeign))
                {
                    var refId = FindSameRowIdOnDestination(item.RelatedTableName.ToString(), rowInSource[item.Name]);

                    if (refId != null)
                    {
                        columnFilter.Add(item, id);
                    }
                }

                var uniqueColumns = Table.Columns.Where(column => column.IsUnique && !column.IsForeign).ToList();

                foreach (var item in uniqueColumns)
                {
                    columnFilter.Add(item, rowInSource[item.Name]);
                }

                var clauses = string.Join(" AND ", uniqueColumns.Select(x => $"{SourceDatabase.BuildClause(x, rowInSource[x.Name])}"));

                var equivalentRow = SourceDatabase.FetchDataByClause(tableName, clauses).SingleOrDefault();

                return equivalentRow?["Id"];
            }

            return null;
        }
    }
}
