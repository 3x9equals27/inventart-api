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

        public async Task<bool> SetUserInfo(Guid user_guid, string firstName, string lastName, string defaultTenant, string defaultLanguage)
        {
            bool success = true;
            var sp_call = "CALL sp_user_settings_edit(@i_user_guid, @i_first_name, @i_last_name, @i_default_tenant, @i_default_language)";
            DynamicParameters sp_params = new DynamicParameters(new { i_user_guid = user_guid, i_first_name = firstName, i_last_name = lastName, i_default_tenant = defaultTenant, i_default_language = defaultLanguage });
            using (var connection = new NpgsqlConnection(_csp.ConnectionString))
            {
                await connection.ExecuteAsync(sp_call, sp_params);
            }
            return success;
        }
        public async Task<List<dynamic>> ListAllUsersRole(string tenantCode)
        {
            var fn_call = "select * from fn_user_list_role_all(@i_tenant_code);";
            DynamicParameters fn_params = new DynamicParameters(new { i_tenant_code = tenantCode });
            using (var connection = new NpgsqlConnection(_csp.ConnectionString))
            {
                var results = (await connection.QueryAsync(fn_call, fn_params)).ToList();
                return results;
            }
        }
        public async Task<bool> EditUserRole(Guid guid, string tenant, string role)
        {
            bool success = true;
            var sp_call = "CALL sp_user_role_change(@i_user_guid, @i_tenant, @i_role)";
            DynamicParameters sp_params = new DynamicParameters(new { i_user_guid = guid, i_tenant = tenant, i_role = role });
            using (var connection = new NpgsqlConnection(_csp.ConnectionString))
            {
                await connection.ExecuteAsync(sp_call, sp_params);
            }
            return success;
        }
    }
}
