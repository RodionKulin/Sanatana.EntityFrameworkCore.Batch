using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Commands
{
    public enum MergeTypeEnum
    {
        Insert,
        Update,
        Upsert,
        DeleteMatched,
        DeleteNotMatched
    }
}
