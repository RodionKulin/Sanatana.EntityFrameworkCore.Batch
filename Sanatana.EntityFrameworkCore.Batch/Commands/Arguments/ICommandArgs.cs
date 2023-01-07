using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping
{
    public interface ICommandArgs
    {
        public List<string> IncludeProperties { get; }

        public List<string> ExcludeProperties { get; }

        public bool ExcludeAllByDefault { get; }

        public ExcludeOptionsEnum ExcludeDbGeneratedByDefault { get;}

        public ExcludeOptionsEnum ExcludePrimaryKeyByDefault { get; }

    }
}
