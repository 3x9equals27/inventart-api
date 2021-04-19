using Inventart.Config;
using Microsoft.Extensions.Options;

namespace Inventart.Services.Singleton
{
    public class ConnectionStringProvider
    {
        private readonly PostgresConfig _config;
        public string ConnectionString { get; }
        public ConnectionStringProvider(IOptions<PostgresConfig> postgresConfig)
        {
            _config = postgresConfig.Value;
            ConnectionString = $"User ID={_config.User};Password={_config.Password};Host={_config.Hostname};Port={_config.Port};Database={_config.Database};SSL Mode=Require;Trust Server Certificate=true;";
        }
    }
}
