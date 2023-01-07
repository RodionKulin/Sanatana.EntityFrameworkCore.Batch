using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using System.Data.Common;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServer.Repositories
{
    public class SqlRepositoryAsync : RepositoryAsync
    {
        public SqlRepositoryAsync(DbContext dbContext)
            : base(dbContext, new SqlParametersService())
        {
        }
    }
}