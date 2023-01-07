using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using Sanatana.EntityFrameworkCore.Batch.SqlServer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories
{
    public class SqlRepositoryFactory : IRepositoryFactory
    {
        protected Func<DbContext> _dbContextFactory;


        public SqlRepositoryFactory(Func<DbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        public virtual IRepository CreateRepository()
        {
            DbContext dbContext = _dbContextFactory();
            return new SqlRepository(dbContext);
        }

        public virtual IRepositoryAsync CreateRepositoryAsync()
        {
            DbContext dbContext = _dbContextFactory();
            return new SqlRepositoryAsync(dbContext);
        }
    }
}
