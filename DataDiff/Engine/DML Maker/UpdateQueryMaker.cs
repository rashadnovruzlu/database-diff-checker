using DataDiff.Engine.Enums;
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
    public class UpdateQueryMaker : QueryMaker
    {
        public UpdateQueryMaker(QueryMakerParameter queryMakerParameter) : base(queryMakerParameter)
        {

        }


        public UpdateQueryResult MakeQuery(IEnumerable<IDictionary<string, object>> source, IEnumerable<IDictionary<string, object>> destination)
        {
            var unrelatedRows = new List<UpdateQueryResult.UnrelatedRow>();

            var updateQueries = new List<UpdateQueryResult.UpdateQuery>();

            var primaryKeyColumn = Table.Columns.Single(x => x.IsPrimaryKey);

            var foreignColumns = Table.Columns.Where(column => !column.IsUnique && column.IsForeign && !column.IsPrimaryKey).ToList();

            foreach (var row in source)
            {
                bool hasRelated = true;

                if (Table.Columns.Any(column => column.IsUnique && column.IsForeign))
                {
                    IEnumerable<IDictionary<string, object>> existingRow = null;

                    IDictionary<string, object> columnFilter = new Dictionary<string, object>();

                    foreach (var item in Table.Columns.Where(column => column.IsUnique && column.IsForeign))
                    {
                        var id = FindSameRowIdOnDestination(item.RelatedTableName.ToString(), row[item.Name]);

                        if (id == null)
                        {
                            hasRelated = false;

                            unrelatedRows.Add(new UpdateQueryResult.UnrelatedRow(item.RelatedTableName.ToString(), item.Name, row[item.Name]));

                            break;
                        }
                        else
                        {
                            existingRow = destination.Where(entry => object.Equals(entry[item.Name], id)).ToList();

                            columnFilter.Add(item.Name, id);

                            if (!existingRow.Any())
                            {
                                break;
                            }

                        }
                    }

                    var uniqueColumns = Table.Columns.Where(column => column.IsUnique && !column.IsForeign).ToList();


                    foreach (var item in uniqueColumns)
                    {
                        columnFilter.Add(item.Name, row[item.Name]);
                    }

                    var filteredRow = destination.SingleOrDefault(entry => columnFilter.All(column => Equals(entry[column.Key], row[column.Key])));

                    if (!hasRelated) continue;

                    if (filteredRow != null)
                    {
                        var differentValueColumns = new Dictionary<string, UpdateQueryResult.UpdateQueryColumnValue>();

                        string updatedColumns = string.Empty;

                        foreach (var filteredRowItem in filteredRow)
                        {
                            foreach (var rowItem in row)
                            {
                                if (rowItem.Key == filteredRowItem.Key)
                                {
                                    if (foreignColumns.Any(x => string.Equals(rowItem.Key, x.Name, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        var item = Table.Columns.Single(x => string.Equals(rowItem.Key, x.Name, StringComparison.OrdinalIgnoreCase));

                                        var id = FindSameRowIdOnDestination(item.RelatedTableName.ToString(), row[item.Name]);

                                        if (id == null)
                                        {
                                            hasRelated = false;

                                            unrelatedRows.Add(new UpdateQueryResult.UnrelatedRow(item.RelatedTableName.ToString(), item.Name, row[item.Name]));

                                            break;
                                        }

                                        if (!Equals(id, filteredRowItem.Value))
                                        {
                                            differentValueColumns.Add(rowItem.Key, new UpdateQueryResult.UpdateQueryColumnValue(id, filteredRowItem.Value));

                                            updatedColumns += $" {DestinationDatabase.BuildClause(rowItem.Key, rowItem.Value)}, ";
                                        }
                                    }
                                    else if (!Equals(rowItem.Value, filteredRowItem.Value))
                                    {
                                        differentValueColumns.Add(rowItem.Key, new UpdateQueryResult.UpdateQueryColumnValue(rowItem.Value, filteredRowItem.Value));

                                        updatedColumns += $" {DestinationDatabase.BuildClause(rowItem.Key, rowItem.Value)}, ";
                                    }
                                }
                            }


                        }

                        if (!hasRelated) continue;

                        if (differentValueColumns.Count == 0) continue;

                        char[] charsToTrim = { ',', ' ' };

                        var sqlUpdateQuery = $"UPDATE {DestinationDatabase.ConvertTableNameToProviderSyntax(TableName)} SET {updatedColumns.TrimEnd(charsToTrim)} WHERE {DestinationDatabase.BuildClause(primaryKeyColumn, filteredRow[primaryKeyColumn.Name])} ;";

                        updateQueries.Add(new UpdateQueryResult.UpdateQuery(sqlUpdateQuery, differentValueColumns));
                    }

                }
                else if (Table.Columns.Any(column => column.IsUnique && !column.IsForeign))
                {

                    var uniqueColumns = Table.Columns.Where(column => column.IsUnique && !column.IsForeign).ToList();

                    var existingRow = destination.FirstOrDefault(entry => uniqueColumns.All(column => string.Equals(entry[column.Name], row[column.Name])));

                    if (existingRow != null)
                    {
                        var differentValueColumns = new Dictionary<string, UpdateQueryResult.UpdateQueryColumnValue>();

                        string updatedColumns = string.Empty;

                        foreach (var filteredRowItem in existingRow)
                        {
                            foreach (var rowItem in row)
                            {
                                if (string.Equals(primaryKeyColumn.Name, rowItem.Key, StringComparison.OrdinalIgnoreCase)) continue;

                                if (rowItem.Key == filteredRowItem.Key)
                                {
                                    if (foreignColumns.Any(x => string.Equals(rowItem.Key, x.Name, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        var item = Table.Columns.Single(x => string.Equals(rowItem.Key, x.Name, StringComparison.OrdinalIgnoreCase));

                                        var id = FindSameRowIdOnDestination(item.RelatedTableName.ToString(), row[item.Name]);

                                        if (id == null)
                                        {
                                            hasRelated = false;

                                            unrelatedRows.Add(new UpdateQueryResult.UnrelatedRow(item.RelatedTableName.ToString(), item.Name, row[item.Name]));

                                            break;
                                        }

                                        if (!Equals(id, filteredRowItem.Value))
                                        {
                                            differentValueColumns.Add(rowItem.Key, new UpdateQueryResult.UpdateQueryColumnValue(id, filteredRowItem.Value));

                                            updatedColumns += $" {DestinationDatabase.BuildClause(rowItem.Key, rowItem.Value)}, ";
                                        }
                                    }
                                    else if (!Equals(rowItem.Value, filteredRowItem.Value))
                                    {
                                        differentValueColumns.Add(rowItem.Key, new UpdateQueryResult.UpdateQueryColumnValue(rowItem.Value, filteredRowItem.Value));

                                        updatedColumns += $" {DestinationDatabase.BuildClause(rowItem.Key, rowItem.Value)}, ";
                                    }
                                }
                            }
                        }

                        if (!hasRelated) continue;

                        if (differentValueColumns.Count == 0) continue;

                        char[] charsToTrim = { ',', ' ' };

                        var sqlUpdateQuery = $"UPDATE {DestinationDatabase.ConvertTableNameToProviderSyntax(TableName)} SET {updatedColumns.TrimEnd(charsToTrim)} WHERE {DestinationDatabase.BuildClause(primaryKeyColumn, existingRow[primaryKeyColumn.Name])} ;";

                        updateQueries.Add(new UpdateQueryResult.UpdateQuery(sqlUpdateQuery, differentValueColumns));
                    }
                }
            }

            return new UpdateQueryResult(updateQueries, unrelatedRows);
        }
    }
}
