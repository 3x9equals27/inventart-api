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
    public class PaintingRepo
    {
        private readonly ConnectionStringProvider _csp;

        public PaintingRepo(ConnectionStringProvider connectionStringProvider)
        {
            _csp = connectionStringProvider;
        }

        public async Task<List<dynamic>> ListAll(string tenant)
        {
            List<dynamic> results = new List<dynamic>();
            var sp_name = "sp_painting_list_all";
            DynamicParameters sql_params = new DynamicParameters(new { i_tenant = tenant });
            //
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                results = (await connection.QueryAsync(sp_name, sql_params, commandType: CommandType.StoredProcedure)).ToList();
            }
            return results;
        }

        public async Task<Guid?> SaveFile(Guid painting, string fileName, byte[] fileBytes)
        {
            Guid? file_guid = null;
            // save to database
            var sp_name = "sp_upload_file_painting";
            DynamicParameters sp_params = new DynamicParameters(new { i_guid = painting, i_name = fileName, i_bytes = fileBytes });
            sp_params.Add("o_file_guid", value: null, DbType.Guid, direction: ParameterDirection.Output);
            //
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                await connection.ExecuteAsync(sp_name, sp_params, commandType: CommandType.StoredProcedure);
                file_guid = sp_params.Get<Guid>("o_file_guid");
            }
            return file_guid;
        }

        public async Task<Guid?> Create(string tenant_code, PaintingDto painting)
        {
            Guid? painting_guid = null;
            // save to database
            var sp_name = "sp_painting_create";
            DynamicParameters sp_params = new DynamicParameters(new
            {
                i_tenant = tenant_code,
                i_name = painting.Name,
                i_author = painting.Author,
                i_description = painting.Description
            });
            sp_params.Add("o_painting_guid", value: null, DbType.Guid, direction: ParameterDirection.Output);
            //
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                await connection.ExecuteAsync(sp_name, sp_params, commandType: CommandType.StoredProcedure);
                painting_guid = sp_params.Get<Guid>("o_painting_guid");
            }
            return painting_guid;
        }
        public async Task Update(string tenant_code, Guid painting_guid, PaintingDto painting)
        {
            // save to database
            var sp_name = "sp_painting_update";
            DynamicParameters sp_params = new DynamicParameters(new
            {
                i_tenant = tenant_code,
                i_guid = painting_guid,
                i_name = painting.Name,
                i_author = painting.Author,
                i_description = painting.Description
            });
            //
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                await connection.ExecuteAsync(sp_name, sp_params, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<dynamic> SelectSingle(string tenant, Guid painting_guid)
        {
            List<dynamic> results = new List<dynamic>();
            var sp_name = "sp_painting_select_single";
            DynamicParameters sql_params = new DynamicParameters(new
            {
                i_tenant = tenant,
                i_guid = painting_guid
            });
            
            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                results = (await connection.QueryAsync(sp_name, sql_params, commandType: CommandType.StoredProcedure)).ToList();
            }
            return results.FirstOrDefault();
        }

    }
}
