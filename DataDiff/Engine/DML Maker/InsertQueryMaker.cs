using DataDiff.Engine.Enums;
using DataDiff.Engine.Extensions;
using DataDiff.Engine.Models;
using DataDiff.Engine.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace DataDiff.Engine.DML_Maker
{
    public class InsertQueryMaker : QueryMaker
    {
        public InsertQueryMaker(QueryMakerParameter queryMakerParameter) : base(queryMakerParameter)
        {

        }


        public InsertQueryResult MakeQuery(IEnumerable<IDictionary<string, object>> source, IEnumerable<IDictionary<string, object>> destination)
        {
            var unrelatedRows = new List<InsertQueryResult.UnrelatedRow>();

            var insertQueries = new List<string>();

            var primaryKeyColumn = Table.Columns.Single(x => x.IsPrimaryKey);

            var foreignColumns = Table.Columns.Where(column => !column.IsUnique && column.IsForeign).ToList();

            foreach (var row in source)
            {
                bool hasRelated = true;

                if (Table.Columns.Any(column => column.IsUnique && column.IsForeign))
                {
                    IEnumerable<IDictionary<string, object>> sameRowOnDestination = null;

                    var conditions = new Dictionary<string, object>();

                    foreach (var item in Table.Columns.Where(column => column.IsUnique && column.IsForeign))
                    {
                        var id = FindSameRowIdOnDestination(item.RelatedTableName.ToString(), row[item.Name]);

                        if (id == null)
                        {
                            hasRelated = false;

                            unrelatedRows.Add(new InsertQueryResult.UnrelatedRow(item.RelatedTableName.ToString(), item.Name, row[item.Name]));

                            break;
                        }
                        else
                        {
                            conditions.Add(item.Name, id);
                            //sameRowOnDestination = destination.Where(entry => object.Equals(entry[item.Name], id)).ToList();

                            //if (!sameRowOnDestination.Any()) break;
                            //else conditions.Add(item.Name, id);

                        }
                    }

                    if (!hasRelated) continue;

                    var uniqueColumns = Table.Columns.Where(column => column.IsUnique && !column.IsForeign).ToList();

                    foreach (var item in uniqueColumns)
                    {
                        conditions.Add(item.Name, row[item.Name]);
                    }

                    var filteredRow = destination.SingleOrDefault(entry => conditions.All(column => Equals(entry[column.Key], column.Value)));

                    if (filteredRow == null)
                    {
                        foreach (var rowItem in row)
                        {
                            if (foreignColumns.Any(x => string.Equals(rowItem.Key, x.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                var item = Table.Columns.Single(x => string.Equals(rowItem.Key, x.Name, StringComparison.OrdinalIgnoreCase));

                                var id = FindSameRowIdOnDestination(item.RelatedTableName.ToString(), row[item.Name]);

                                if (id == null)
                                {
                                    hasRelated = false;

                                    unrelatedRows.Add(new InsertQueryResult.UnrelatedRow(item.RelatedTableName.ToString(), item.Name, row[item.Name]));

                                    break;
                                }

                                row[item.Name] = id;
                            }
                        }

                        if (!hasRelated) continue;

                        var tableColumns = string.Join(", ", row.SkipFirst().Select(kv => $" {DestinationDatabase.ConvertColumnNameToProviderSyntax(kv.Key)}"));

                        var tableColumnsValues = string.Join(", ", row.SkipFirst().Select(kv => kv.Value.GetColumnType() == ColumnDataTypes.Character ?
                        $"'{(conditions.Any(x => x.Key == kv.Key) ? conditions.Single(x => x.Key == kv.Key).Value : kv.Value)}'" :
                        (conditions.Any(x => x.Key == kv.Key) ? conditions.Single(x => x.Key == kv.Key).Value : kv.Value).ToString()));

                        var sqlInsertQuery = $"INSERT INTO  {DestinationDatabase.ConvertTableNameToProviderSyntax(TableName)}  ({tableColumns}) VALUES ({tableColumnsValues});";

                        insertQueries.Add(sqlInsertQuery);
                    }

                }
                else if (Table.Columns.Any(column => column.IsUnique && !column.IsForeign))
                {

                    var uniqueColumns = Table.Columns.Where(column => column.IsUnique && !column.IsForeign).ToList();
                    var existingRow = destination.FirstOrDefault(entry => uniqueColumns.All(column => Equals(entry[column.Name], row[column.Name])));

                    if (existingRow == null)
                    {
                        foreach (var rowItem in row)
                        {
                            if (foreignColumns.Any(x => string.Equals(rowItem.Key, x.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                var item = Table.Columns.Single(x => string.Equals(rowItem.Key, x.Name, StringComparison.OrdinalIgnoreCase));

                                var id = FindSameRowIdOnDestination(item.RelatedTableName.ToString(), row[item.Name]);

                                if (id == null)
                                {
                                    hasRelated = false;

                                    unrelatedRows.Add(new InsertQueryResult.UnrelatedRow(item.RelatedTableName.ToString(), item.Name, row[item.Name]));

                                    break;
                                }

                                row[item.Name] = id;
                            }
                        }

                        if (!hasRelated) continue;

                        var tableColumns = string.Join(", ", row.SkipFirst().Select(kv => $"{DestinationDatabase.ConvertColumnNameToProviderSyntax(kv.Key)}"));

                        var tableColumnsValues = string.Join(", ", row.SkipFirst().Select(kv => kv.Value.GetColumnType() == ColumnDataTypes.Character ? $"'{kv.Value}'" : kv.Value.ToString()));

                        var sqlInsertQuery = $"INSERT INTO {DestinationDatabase.ConvertTableNameToProviderSyntax(TableName)} ({tableColumns}) VALUES ({tableColumnsValues});";

                        insertQueries.Add(sqlInsertQuery);
                    }
                }
            }

            return new InsertQueryResult(insertQueries, unrelatedRows);
        }
    }
}
