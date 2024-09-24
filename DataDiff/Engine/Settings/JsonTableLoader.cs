using DataDiff.Engine.Models;
using System.Text.Json;

namespace DataDiff.Engine.Settings
{
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public OperationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }
    }
    public class JsonTableLoader : ITableLoader
    {
        public List<SystemDatabaseTable> Tables { get { return _tables; } }

        private List<SystemDatabaseTable> _tables;

        public const string FilePath = "";

        public const string FileName = "diff-checker.json";

        public string FullPath = Path.Combine(FileName, FilePath);

        public JsonTableLoader()
        {
            _tables = ReadJsonFileAsync(FullPath).GetAwaiter().GetResult();
        }

        public async Task<OperationResult> AddTable(string name, string description, string dependentTableName)
        {
            if (_tables.Any(x => x.Name == name)) return new OperationResult(true, $"{name} table already exists.");

            Tables.Add(new SystemDatabaseTable() { Name = name, Description = description, DependentTableName = dependentTableName });

            await SaveToFile();

            return new OperationResult(true, string.Empty);
        }
        public async Task<OperationResult> AddColumnToTable(string tableName, SystemDatabaseTableColumn column)
        {
            if (!_tables.Any(x => x.Name == tableName)) return new OperationResult(true, $"{tableName} table not exists.");

            if (_tables.Any(x => x.Name == tableName && x.Columns.Any(c => c.Name == column.Name))) return new OperationResult(true, $"{column.Name} column already exists..");

            _tables.Single(x => x.Name == tableName).Columns.Add(column);

            await SaveToFile();

            return new OperationResult(true, string.Empty);
        }

        async Task WriteJsonToFileAsync(string filePath, string jsonString)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(jsonString);
            }
        }

        async Task<List<SystemDatabaseTable>> ReadJsonFileAsync(string filePath)
        {
            if (!File.Exists(FullPath))
            {
                string jsonString = JsonSerializer.Serialize(new List<SystemDatabaseTable>(), new JsonSerializerOptions { WriteIndented = true });

                await WriteJsonToFileAsync(FullPath, jsonString);
            }

            var systemDatabaseTable = new List<SystemDatabaseTable>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string jsonString = await reader.ReadToEndAsync();

                systemDatabaseTable = JsonSerializer.Deserialize<List<SystemDatabaseTable>>(jsonString);
            }

            return systemDatabaseTable;
        }

        public async Task SaveToFile()
        {
            string jsonString = JsonSerializer.Serialize(Tables, new JsonSerializerOptions { WriteIndented = true });

            await WriteJsonToFileAsync(FullPath, jsonString);
        }
    }
}
