using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch
{
    public static class DbCommandExtentions
    {
        public static DbParameter CreateCommand(this DbCommand dbCommand, string parameterName, object value)
        {
            DbParameter parameter = dbCommand.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            return parameter;
        }
    }
}
