using Microsoft.Data.SqlClient;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServer
{
    public class SqlParametersService : IDbParametersService
    {
        //properties
        public int MaxParametersPerCommand { get; } = 2100;


        //parameter methods
        public virtual DbParameter GetDbParameter(string parameterName, object value)
        {
            return new SqlParameter(parameterName, value);
        }

        public virtual DbParameter GetDbParameter(string parameterName, object value, MappedProperty mappedProperty)
        {
            var parameter = new SqlParameter(parameterName, value);

            if (mappedProperty.ConfiguredSqlType != null)
            {
                parameter.SqlDbType = GetSqlDbType(mappedProperty);
            }

            return parameter;
        }

        public virtual DbParameter GetDbParameter(string parameterName, DataTable value, string tvpTypeName)
        {
            SqlParameter tableParam = new SqlParameter(parameterName, value);
            tableParam.SqlDbType = SqlDbType.Structured;
            tableParam.TypeName = tvpTypeName;

            return tableParam;
        }

        public virtual SqlDbType GetSqlDbType(MappedProperty mappedProperty)
        {
            string sqlType = mappedProperty.ConfiguredSqlType;
            if (sqlType.ToLower().Contains("nvarchar"))
            {
                sqlType = "nvarchar";
            }
            var sqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), sqlType, true);
            return sqlDbType;
        }


        //format methods
        public virtual string FormatTableName(string tableName)
        {
            return tableName;
        }
        public virtual string FormatColumnName(string parameterName)
        {
            return $"[{parameterName}]";
        }
        public virtual string FormatParameterName(string parameterName)
        {
            return $"@{parameterName}";
        }
        public virtual string FormatOutputParameter(string parameterName)
        {
            //it works with insert and update commands
            return $"INSERTED.{parameterName}";
        }
        public virtual string FormatExpression(string expression)
        {
            return expression;
        }
        public string FormatBoolean(bool value)
        {
            return value ? "1" : "0";
        }
    }
}
