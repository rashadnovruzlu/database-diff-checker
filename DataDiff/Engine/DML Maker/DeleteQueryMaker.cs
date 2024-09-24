using DataDiff.Engine.Enums;
using DataDiff.Engine.Models;
using DataDiff.Engine.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataDiff.Engine.Params.InsertQueryResult;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace DataDiff.Engine.DML_Maker
{
    public class DeleteQueryMaker : QueryMaker
    {
        public DeleteQueryMaker(QueryMakerParameter queryMakerParameter) : base(queryMakerParameter)
        {

        }


        public DeleteQueryResult MakeQuery(IEnumerable<IDictionary<string, object>> source, IEnumerable<IDictionary<string, object>> destination)
        {
            var unrelatedRows = new List<DeleteQueryResult.UnrelatedRow>();

            var deleteQueries = new List<string>();

            var primaryKeyColumn = Table.Columns.Single(x => x.IsPrimaryKey);

            foreach (var row in source)
            {
                bool hasRelated = true;

                if (Table.Columns.Any(column => column.IsUnique && column.IsForeign))
                {
                    IEnumerable<IDictionary<string, object>> existingRow = null;

                    var conditions = new Dictionary<string, object>();

                    foreach (var item in Table.Columns.Where(column => column.IsUnique && column.IsForeign))
                    {
                        var id = FindSameRowIdOnSource(item.RelatedTableName.ToString(), row[item.Name]);

                        if (id == null)
                        {
                            hasRelated = false;

                            unrelatedRows.Add(new DeleteQueryResult.UnrelatedRow(item.RelatedTableName.ToString(), item.Name, row[item.Name]));

                            break;
                        }
                        else
                        {
                            existingRow = destination.Where(entry => object.Equals(entry[item.Name], id)).ToList();

                            conditions.Add(item.Name, id);

                            if (!existingRow.Any())
                            {
                                break;
                            }

                        }
                    }

                    var uniqueColumns = Table.Columns.Where(column => column.IsUnique && !column.IsForeign).ToList();

                    foreach (var item in uniqueColumns)
                    {
                        conditions.Add(item.Name, row[item.Name]);
                    }

                    var filteredRow = destination.SingleOrDefault(entry => conditions.All(column => Equals(entry[column.Key], column.Value)));

                    if (!hasRelated) continue;

                    if (filteredRow == null)
                    {
                        var sqlInsertQuery = $"DELETE FROM {DestinationDatabase.ConvertTableNameToProviderSyntax(TableName)} WHERE  {DestinationDatabase.BuildClause(primaryKeyColumn, row[primaryKeyColumn.Name])} ;";

                        deleteQueries.Add(sqlInsertQuery);
                    }

                }
                else if (Table.Columns.Any(column => column.IsUnique && !column.IsForeign))
                {
                    var uniqueColumns = Table.Columns.Where(column => column.IsUnique && !column.IsForeign).ToList();

                    var existingRow = destination.FirstOrDefault(entry => uniqueColumns.All(column => string.Equals(entry[column.Name], row[column.Name])));

                    if (existingRow == null)
                    {
                        var sqlInsertQuery = $"DELETE FROM {DestinationDatabase.ConvertTableNameToProviderSyntax(TableName)} WHERE {DestinationDatabase.BuildClause(primaryKeyColumn, row[primaryKeyColumn.Name])} ;";

                        deleteQueries.Add(sqlInsertQuery);
                    }
                }
            }

            return new DeleteQueryResult(deleteQueries, unrelatedRows);
        }
    }
}
