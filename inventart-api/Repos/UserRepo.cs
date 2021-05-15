using Dapper;
using Inventart.Services.Singleton;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Inventart.Repos
{
    public class UserRepo
    {
        private readonly ConnectionStringProvider _csp;

        public UserRepo(ConnectionStringProvider connectionStringProvider)
        {
            _csp = connectionStringProvider;
        }

        public async Task<dynamic> GetUserInfo(Guid guid)
        {
            var fn_call = "select * from fn_user_info(@i_guid);";
            DynamicParameters fn_params = new DynamicParameters(new { i_guid = guid });
            using (var connection = new NpgsqlConnection(_csp.ConnectionString))
            {
                var results = (await connection.QueryAsync(fn_call, fn_params)).ToList();
                if (results.Count > 0)
                {
                    dynamic userInfo = results.First();
                    return userInfo;
                }
            }
            return null;
        }

        public bool SetUserInfo(Guid user_guid, string firstName, string lastName, string defaultTenant, string defaultLanguage)
        {
            bool success = true;
            var sp_call = "CALL sp_user_settings_edit(@i_user_guid, @i_first_name, @i_last_name, @i_default_tenant, @i_default_language)";
            DynamicParameters sp_params = new DynamicParameters(new { i_user_guid = user_guid, i_first_name = firstName, i_last_name = lastName, i_default_tenant = defaultTenant, i_default_language = defaultLanguage });
            using (var connection = new NpgsqlConnection(_csp.ConnectionString))
            {
                connection.Execute(sp_call, sp_params);
            }
            return success;
        }
    }
}
