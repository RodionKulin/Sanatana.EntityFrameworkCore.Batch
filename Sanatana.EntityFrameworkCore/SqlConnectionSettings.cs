using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore
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
