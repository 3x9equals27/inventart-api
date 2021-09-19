using Dapper;
using Inventart.Models.RepoOutputs;
using Inventart.Services.Singleton;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Inventart.Repos
{
    public class FileRepo
    {
        private readonly ConnectionStringProvider _csp;

        public FileRepo(ConnectionStringProvider connectionStringProvider)
        {
            _csp = connectionStringProvider;
        }

        public async Task<RepoFile> GetFile(Guid fileGuid)
        {
            RepoFile file = new RepoFile();
            
            List<dynamic> results = new List<dynamic>();
            var sql = "SELECT name, bytes FROM [file] WHERE guid = @guid";

            using (var connection = new SqlConnection(_csp.ConnectionString))
            {
                results = (await connection.QueryAsync(sql, new { guid = fileGuid })).ToList();
            }

            if (results.Count != 1) return null;

            file.FileBytes = results[0].bytes;
            file.FileName  = results[0].name;
            
            return file;
        }
    }
}
