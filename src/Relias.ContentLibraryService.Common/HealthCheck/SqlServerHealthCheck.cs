using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
namespace Relias.ContentLibraryService.Common.Services
{
    public class SqlServerHealthCheck : IDependencyHealthCheck
    {
        private const string HealthQuery = "SELECT 1;";

        public string ServiceName => "Sql Server";

        private readonly string _connectionString;

        public SqlServerHealthCheck(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task CheckServiceHealth()
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = connection.CreateCommand();
            command.CommandText = HealthQuery;

            await command.ExecuteScalarAsync().ConfigureAwait(false);
        } 
    }
}