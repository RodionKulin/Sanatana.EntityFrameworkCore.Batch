using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.EntityFrameworkCore.Batch
{
    public class SqlConnectionSettings
    {
        //properties
        public virtual string ConnectionString { get; set; }
        public virtual string Schema { get; set; }


        //init
        public SqlConnectionSettings()
        {

        }
        public SqlConnectionSettings(string connectionString, string schema)
        {
            ConnectionString = connectionString;
            Schema = schema;
        }
    }
}
