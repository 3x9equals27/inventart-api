using Dapper;
using Inventart.Models.ControllerInputs;
using Inventart.Services.Singleton;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Inventart.Repos
{
    public class DiagRepo
    {
        private readonly ConnectionStringProvider _csp;

        public DiagRepo(ConnectionStringProvider connectionStringProvider)
        {
            _csp = connectionStringProvider;
        }

        public async Task<List<dynamic>> ListAllDiagnostic(string tenant)
        {
            List<dynamic> results = new List<dynamic>();
            var sql = "EXEC sp_diagnostico_list_all @i_tenant";
            DynamicParameters sql_params = new DynamicParameters(new { i_tenant = tenant });
            //
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                results = (await connection.QueryAsync(sql, sql_params)).ToList();
            }
            return results;
        }
        public async Task<Guid?> SaveFile(Guid diagnostic, string fileName, byte[] fileBytes)
        {
            Guid? file_guid = null;
            // save to database
            var sp_name = "sp_upload_file_diagnostico";
            DynamicParameters sp_params = new DynamicParameters(new { i_guid = diagnostic, i_name = fileName, i_bytes = fileBytes });
            sp_params.Add("o_file_guid", value: null, DbType.Guid, direction: ParameterDirection.Output);
            //
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                await connection.ExecuteAsync(sp_name, sp_params, commandType: CommandType.StoredProcedure);
                file_guid = sp_params.Get<Guid>("o_file_guid");
            }
            return file_guid;
        }
        public async Task<Guid?> DiagnosticCreate(string tenant_code, DiagnosticCreate diagnostic)
        {
            Guid? diagnostic_guid = null;
            // save to database
            var sp_name = "sp_diagnostico_create";
            DynamicParameters sp_params = new DynamicParameters(new { i_tenant = tenant_code, i_description = diagnostic.Description });
            sp_params.Add("o_diagnostico_guid", value: null, DbType.Guid, direction: ParameterDirection.Output);
            //
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                await connection.ExecuteAsync(sp_name, sp_params, commandType: CommandType.StoredProcedure);
                diagnostic_guid = sp_params.Get<Guid>("o_diagnostico_guid");
            }
            return diagnostic_guid;
        }
    }
}
