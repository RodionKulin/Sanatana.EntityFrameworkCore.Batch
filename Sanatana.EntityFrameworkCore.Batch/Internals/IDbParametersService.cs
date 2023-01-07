using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Internals
{
    public interface IDbParametersService
    {
        int MaxParametersPerCommand { get; }
        DbParameter GetDbParameter(string parameterName, object value);
        DbParameter GetDbParameter(string parameterName, object value, MappedProperty mappedProperty);
        DbParameter GetDbParameter(string parameterName, DataTable data, string tvpTypeName);

        string FormatTableName(string tableName);
        string FormatColumnName(string parameterName);
        string FormatParameterName(string parameterName);
        string FormatOutputParameter(string parameterName);
        string FormatExpression(string expression);

        string FormatBoolean(bool value);
    }
}
