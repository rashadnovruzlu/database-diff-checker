using DataDiff.Engine.DML_Maker;
using DataDiff.Engine.Enums;
using DataDiff.Engine.Models;
using DataDiff.Engine.Settings;
using DataDiff.Providers;
using System.Text;

namespace API
{
    internal class Program
    {
        static string userTableName = "User";
        static string personTableName = "Person";

        static string tableName = "Person";

        static string sourceDatabaseName = "R_1";

        static string destinationDatabaseName = "R_2";

        static ITableLoader tableLoader = new JsonTableLoader();

        static ISqlProvider sourceDatabase = new PostgreSQLProvider(sourceDatabaseName, "localhost:5432", "postgres", "123");

        static ISqlProvider destinationDatabase = new PostgreSQLProvider(destinationDatabaseName, "localhost:5432", "postgres", "123");

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            AddTableInfo().GetAwaiter().GetResult();

            var sourceDatabaseTable = sourceDatabase.FetchData(tableName);

            var destinationDatabaseTable = destinationDatabase.FetchData(tableName);

            WriteInsertQueries(sourceDatabaseTable, destinationDatabaseTable);

            WriteDeleteQueries(sourceDatabaseTable, destinationDatabaseTable);

            WriteUpdateQueries(sourceDatabaseTable, destinationDatabaseTable);

            Console.ReadLine();
        }

        private static void WriteInsertQueries(List<IDictionary<string, object>> resultDbOne, List<IDictionary<string, object>> resultDbTwo)
        {

            InsertQueryMaker insertQuerMaker = new InsertQueryMaker(new QueryMakerParameter(tableLoader, sourceDatabase, destinationDatabase, tableName));

            var insertQueries = insertQuerMaker.MakeQuery(resultDbOne, resultDbTwo);

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"{Environment.NewLine}------------------------------INSERT QUERIES BEGIN------------------------------{Environment.NewLine}");

            Console.ForegroundColor = ConsoleColor.White;

            foreach (var query in insertQueries.Queries)
            {
                Console.WriteLine(query);
            }

            if (insertQueries.Queries.Count == 0)
            {
                Console.WriteLine($"There are no insert queries");
            }

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"{Environment.NewLine}------------------------------INSERT QUERIES END------------------------------{Environment.NewLine}");
        }

        private static void WriteDeleteQueries(List<IDictionary<string, object>> resultDbOne, List<IDictionary<string, object>> resultDbTwo)
        {
            DeleteQueryMaker deleteQuerMaker = new DeleteQueryMaker(new QueryMakerParameter(tableLoader, sourceDatabase, destinationDatabase, tableName));

            var deleteQueries = deleteQuerMaker.MakeQuery(resultDbTwo, resultDbOne);

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}------------------------------DELETE QUERIES BEGIN------------------------------{Environment.NewLine}");

            Console.ForegroundColor = ConsoleColor.White;

            foreach (var query in deleteQueries.Queries)
            {
                Console.WriteLine(query);
            }

            if (deleteQueries.Queries.Count == 0)
            {
                Console.WriteLine($"There are no delete queries");
            }

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{Environment.NewLine}------------------------------DELETE QUERIES END------------------------------{Environment.NewLine}");
        }

        private static void WriteUpdateQueries(List<IDictionary<string, object>> resultDbOne, List<IDictionary<string, object>> resultDbTwo)
        {
            UpdateQueryMaker updateQuerMaker = new UpdateQueryMaker(new QueryMakerParameter(tableLoader, sourceDatabase, destinationDatabase, tableName));

            var updateQueries = updateQuerMaker.MakeQuery(resultDbOne, resultDbTwo);


            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}------------------------------UPDATE QUERIES BEGIN------------------------------{Environment.NewLine}");

            Console.ForegroundColor = ConsoleColor.White;

            foreach (var item in updateQueries.Queries)
            {
                Console.WriteLine(item.Query);
            }

            if (updateQueries.Queries.Count == 0)
            {
                Console.WriteLine($"There are no update queries");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"{Environment.NewLine}------------------------------UPDATE QUERIES END------------------------------{Environment.NewLine}");
        }

        private static async Task AddTableInfo()
        {
            await AddPersonTableInfo();

            await AddUserTableInfo();
        }

        private static async Task AddUserTableInfo()
        {
            await tableLoader.AddTable(userTableName, userTableName, null);

            var userIdColumns = new Dictionary<ColumnAttributeType, object>();
            userIdColumns.Add(ColumnAttributeType.IsPrimaryKey, true);

            var userId = new SystemDatabaseTableColumn()
            {
                DataType = ColumnDataTypes.Number,
                Name = "Id",
                TableName = userTableName,
                ColumnAttributes = userIdColumns
            };


            var userPersonIdColumns = new Dictionary<ColumnAttributeType, object>();
            userPersonIdColumns.Add(ColumnAttributeType.IsUnique, true);
            userPersonIdColumns.Add(ColumnAttributeType.IsForeign, true);

            var userPersonId = new SystemDatabaseTableColumn()
            {
                DataType = ColumnDataTypes.Number,
                Name = "PersonId",
                TableName = userTableName,
                RelatedTableName = personTableName,
                ColumnAttributes = userPersonIdColumns
            };


            var userUsernameColumns = new Dictionary<ColumnAttributeType, object>();
            userUsernameColumns.Add(ColumnAttributeType.IsUnique, true);

            var userUsername = new SystemDatabaseTableColumn()
            {
                DataType = ColumnDataTypes.Character,
                Name = "Username",
                TableName = userTableName,
                ColumnAttributes = userUsernameColumns
            };


            await tableLoader.AddColumnToTable(userTableName, userId);
            await tableLoader.AddColumnToTable(userTableName, userPersonId);
            await tableLoader.AddColumnToTable(userTableName, userUsername);
        }

        private static async Task AddPersonTableInfo()
        {
            await tableLoader.AddTable(personTableName, userTableName, null);

            var personIdColumns = new Dictionary<ColumnAttributeType, object>();
            personIdColumns.Add(ColumnAttributeType.IsPrimaryKey, true);

            var personId = new SystemDatabaseTableColumn()
            {
                DataType = ColumnDataTypes.Number,
                Name = "Id",
                TableName = personTableName,
                ColumnAttributes = personIdColumns
            };

            var personPinColumns = new Dictionary<ColumnAttributeType, object>();
            personPinColumns.Add(ColumnAttributeType.IsUnique, true);

            var personPin = new SystemDatabaseTableColumn()
            {
                DataType = ColumnDataTypes.Character,
                Name = "PIN",
                TableName = personTableName,
                ColumnAttributes = personPinColumns
            };

            await tableLoader.AddColumnToTable(personTableName, personId);
            await tableLoader.AddColumnToTable(personTableName, personPin);

        }
    }
}
