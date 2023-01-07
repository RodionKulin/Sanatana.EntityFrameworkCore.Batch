using Npgsql;
using NpgsqlTypes;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql
{
    public class PostgreParametersService : IDbParametersService
    {
        //properties
        public int MaxParametersPerCommand { get; } = 65535;


        //parameter methods
        public virtual DbParameter GetDbParameter(string parameterName, object value)
        {
            return new NpgsqlParameter(parameterName, value);
        }

        public virtual DbParameter GetDbParameter(string parameterName, object value, MappedProperty mappedProperty)
        {
            NpgsqlParameter parameter = new NpgsqlParameter(parameterName, value);

            if (mappedProperty.ConfiguredSqlType != null)
            {
                parameter.NpgsqlDbType = GetDbType(mappedProperty);
            }

            return parameter;
        }

        public virtual DbParameter GetDbParameter(string parameterName, DataTable value, string tvpTypeName)
        {
            throw new NotImplementedException("Npgsql does not have DbType for TVP");
        }

        public virtual NpgsqlDbType GetDbType(MappedProperty mappedProperty)
        {
            string dbType = mappedProperty.ConfiguredSqlType;
            dbType = dbType.ToLower();
            if (dbType.Contains("varchar") ||
                dbType.Contains("character varying"))
            {
                dbType = "varchar";
            }
            else if (dbType == "timestamp with time zone")
            {
                dbType = "timestamptz";
            }

            return (NpgsqlDbType)Enum.Parse(typeof(NpgsqlDbType), dbType, true);
        }


        //format methods
        public virtual string FormatTableName(string tableName)
        {
            return tableName.Replace("[", "\"")
                .Replace("]", "\"");
        }
        public virtual string FormatColumnName(string parameterName)
        {
            return $"\"{parameterName}\"";
        }
        public virtual string FormatParameterName(string parameterName)
        {
            return $"@{parameterName}";
        }
        public virtual string FormatOutputParameter(string parameterName)
        {
            return $"\"{parameterName}\"";
        }
        public virtual string FormatExpression(string expression)
        {
            return expression.Replace("[", "\"")
                .Replace("]", "\"")
                .Replace("as uniqueidentifier", "as uuid")
                .Replace("as datetime2", "as timestamp");
        }

        public string FormatBoolean(bool value)
        {
            return value ? "true" : "false";
        }
    }
}
