using Dapper;
using Inventart.Services.Singleton;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Inventart.Repos
{
    public class AuthRepo
    {
        private readonly ConnectionStringProvider _csp;

        public AuthRepo(ConnectionStringProvider connectionStringProvider)
        {
            _csp = connectionStringProvider;
        }

        public Guid UserRegistration(string email, string passwordHash, string firstName, string lastName, string defaultTenant)
        {
            Guid? verification_guid = null;
            var sp_name = "sp_user_registration";
            DynamicParameters sp_params = new DynamicParameters(new { i_email = email, i_password_hash = passwordHash, i_first_name = firstName, i_last_name = lastName, i_default_tenant = defaultTenant });
            sp_params.Add("o_verification_guid", value: null, DbType.Guid, direction: ParameterDirection.Output);
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                connection.Execute(sp_call, sp_params, commandType: CommandType.StoredProcedure);
                verification_guid = sp_params.Get<Guid>("o_verification_guid");
            }
            return verification_guid.Value;
        }

        public bool UserVerification(Guid verificationGuid)
        {
            bool success = false;
            var sp_call = "EXEC sp_user_verification @i_verification_guid, @o_success";
            DynamicParameters sp_params = new DynamicParameters(new { i_verification_guid = verificationGuid });
            sp_params.Add("@o_success", value: null, DbType.Boolean, direction: ParameterDirection.Output);
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                connection.Execute(sp_call, sp_params);
                success = sp_params.Get<bool>("o_success");
            }
            return success;
        }

        public async Task<dynamic> UserForLogin(string email)
        {
            var sp_call = "EXEC sp_user_for_login @i_email";
            DynamicParameters sp_params = new DynamicParameters(new { i_email = email });
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                var results = (await connection.QueryAsync(sp_call, sp_params)).ToList();
                if (results.Count > 0)
                    return results.First();
            }
            return null;
        }
        public async Task<dynamic> UserInfo(Guid guid)
        {
            var sp_call = "EXEC sp_user_info @i_guid";
            DynamicParameters sp_params = new DynamicParameters(new { i_guid = guid });
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                var results = (await connection.QueryAsync(sp_call, sp_params)).ToList();
                if (results.Count > 0)
                {
                    dynamic userInfo = results.First();
                    return userInfo;
                }
            }
            return null;
        }
        public async Task<List<dynamic>> UserTenants(Guid guid)
        {
            var sp_call = "EXEC sp_user_tenants @i_guid";
            DynamicParameters sp_params = new DynamicParameters(new { i_guid = guid });
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                var results = (await connection.QueryAsync(sp_call, sp_params)).ToList();
                return results;
            }
        }
        public async Task<dynamic> UserTenant(Guid guid, string tenantCode)
        {
            var sp_call = "EXEC sp_user_tenant @i_guid, @i_code";
            DynamicParameters sp_params = new DynamicParameters(new { i_guid = guid, i_code = tenantCode });
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                var results = (await connection.QueryAsync(sp_call, sp_params)).ToList();
                return results.FirstOrDefault();
            }
        }

        public async Task<string> RoleOfUserTenant(Guid userGuid, string tenantCode)
        {
            string role = null;
            var sp_call = "EXEC sp_user_tenant_role @i_user_guid, @i_tenant_code";
            DynamicParameters sp_params = new DynamicParameters(new { i_user_guid = userGuid, i_tenant_code = tenantCode });
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                role = (await connection.ExecuteScalarAsync<string>(sp_call, sp_params));
            }
            return role;
        }

        public Guid? PasswordResetStep1(string email)
        {
            Guid? password_reset_guid = null;
            var sp_call = "EXEC sp_auth_password_reset_step1 @i_email, @o_guid";
            DynamicParameters sp_params = new DynamicParameters(new { i_email = email });
            sp_params.Add("@o_guid", value: null, DbType.Guid, direction: ParameterDirection.Output);
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                connection.Execute(sp_call, sp_params);
                password_reset_guid = sp_params.Get<Guid?>("o_guid");
            }
            return password_reset_guid;
        }
        public bool PasswordResetStep2a(Guid password_reset_guid)
        {
            bool exists = false;
            var sp_call = "EXEC sp_auth_password_reset_step2a @i_password_reset_guid, @o_exists";
            DynamicParameters sp_params = new DynamicParameters(new { i_password_reset_guid = password_reset_guid });
            sp_params.Add("@o_exists", value: null, DbType.Boolean, direction: ParameterDirection.Output);
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                connection.Execute(sp_call, sp_params);
                exists = sp_params.Get<bool>("o_exists");
            }
            return exists;
        }
        public bool PasswordResetStep2b(Guid password_reset_guid, string password_hash)
        {
            bool success = false;
            var sp_call = "EXEC sp_auth_password_reset_step2b @i_password_reset_guid, @i_password_hash, @o_success";
            DynamicParameters sp_params = new DynamicParameters(new { i_password_reset_guid = password_reset_guid, i_password_hash = password_hash });
            sp_params.Add("@o_success", value: null, DbType.Boolean, direction: ParameterDirection.Output);
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                connection.Execute(sp_call, sp_params);
                success = sp_params.Get<bool>("o_success");
            }
            return success;
        }
    }
}
