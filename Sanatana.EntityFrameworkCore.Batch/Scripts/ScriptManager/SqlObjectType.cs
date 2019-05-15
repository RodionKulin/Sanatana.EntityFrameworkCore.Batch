using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Scripts
{
    public enum SqlObjectType
    { 
        StoredProcedure,
        Table,
        AlterTable,
        Index,
        Function,
        TableType, 
        Unknown 
    }
}
