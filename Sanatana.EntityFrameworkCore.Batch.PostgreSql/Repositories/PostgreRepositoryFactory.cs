using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories
{
    public class PostgreRepositoryFactory : IRepositoryFactory
    {
        protected Func<DbContext> _dbContextFactory;


        public PostgreRepositoryFactory(Func<DbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        public virtual IRepository CreateRepository()
        {
            DbContext dbContext = _dbContextFactory();
            return new PostgreRepository(dbContext);
        }

        public virtual IRepositoryAsync CreateRepositoryAsync()
        {
            DbContext dbContext = _dbContextFactory();
            return new PostgreRepositoryAsync(dbContext);
        }
    }
}
