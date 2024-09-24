using Npgsql;
using System.Data;
using System.Data.SqlClient;

namespace DataDiff.Providers
{
    public sealed class DbSettings
    {
        public string? Server { get; set; }
        public string? Database { get; set; }
        public string? UserId { get; set; }
        public string? Password { get; set; }
    }

    public sealed class DataContext
    {
        public readonly DbSettings _dbSettings;

        private readonly SqlProviders _sqlProvider;

        private IDbConnection _dbConnection;

        public IDbConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    _dbConnection = CreateConnection();
                }

                return _dbConnection!;
            }
        }
        public DataContext(SqlProviders sqlProvider, DbSettings dbSettings)
        {
            _sqlProvider = sqlProvider;

            _dbSettings = dbSettings;
        }

        private IDbConnection CreateConnection()
        {
            var connectionString = $"Host={_dbSettings.Server}; Database={_dbSettings.Database}; Username={_dbSettings.UserId}; Password={_dbSettings.Password};";

            if (_sqlProvider == SqlProviders.PostgreSQL)
            {
                return new NpgsqlConnection(connectionString);
            }
            else if (_sqlProvider == SqlProviders.MSSQL)
            {
                return new System.Data.SqlClient.SqlConnection(connectionString);
            }

            return new SqlConnection(connectionString);
        }


    }

}

