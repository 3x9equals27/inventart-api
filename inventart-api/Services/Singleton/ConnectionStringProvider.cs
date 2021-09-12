using Inventart.Config;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace Inventart.Services.Singleton
{
    public class ConnectionStringProvider
    {
        private readonly SqlServerConfig _config;
        public string ConnectionString { get; }

        public ConnectionStringProvider(IOptions<SqlServerConfig> config)
        {
            _config = config.Value;
            ConnectionString = new SqlConnectionStringBuilder()
            {
                DataSource = _config.DataSource,
                InitialCatalog = _config.InitialCatalog,
                UserID = _config.UserID,
                Password = _config.Password
            }.ConnectionString;
        }
    }
}
