using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Commands.Merge
{
    public enum MergeType
    {
        Insert,
        Update,
        Upsert,
        DeleteMatched,
        DeleteNotMatched
    }
}
