using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch
{
    public static class DbConnectionExtensions
    {
        public static DbCommand CreateCommand(this DbConnection connection, string commandText)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            return command;
        }

        public static DbCommand CreateCommand(this DbConnection connection, string commandText, DbTransaction transaction)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.Transaction = transaction;
            return command;
        }
    }
}
