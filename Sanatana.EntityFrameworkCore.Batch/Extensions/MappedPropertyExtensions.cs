using Sanatana.EntityFrameworkCore.Batch.ColumnMapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Sanatana.EntityFrameworkCore.Batch
{
    public static class MappedPropertyExtensions
    {
        public static SqlDbType GetSqlDbType(this MappedProperty mappedProperty)
        {
            string sqlType = mappedProperty.ConfiguredSqlType;
            sqlType = sqlType.ToLower().Replace("nvarchar(max)", "nvarchar");
            var sqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), sqlType, true);
            return sqlDbType;
        }
    }
}
