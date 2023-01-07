using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Commands.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Commands
{
    public interface IMergeCommand<TEntity> : IExecutableCommand
        where TEntity : class
    {
        //properties
        /// <summary>
        /// Type of the Table Valued Parameter that is expected to be already created on SQL server before executing merge, describing order or Source columns. 
        /// Required when using merge constructor with TVP, not required if using SqlParameters constructor.
        /// </summary>
        public string SqlTVPTypeName { get; set; }
        /// <summary>
        /// Name of the Table Valued Parameter that defaults to @Table. This can be any string.
        /// Required when using merge constructor with TVP, not required if using SqlParameters constructor.
        /// </summary>
        public string SqlTVPParameterName { get; set; }
        /// <summary>
        /// Target table name taken from EntityFramework settings by default. Can be changed manually.
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Enable transaction usage when there is more then a single merge request.
        /// Merge requests get splitted into batches when max number of SqlParameters per query is reached. 
        /// Each property of an entity is passed as DbParameter if not using TVP.
        /// You might want to prevent Merge from creating transaction if using ambient transaction from your code.
        /// Enabled by default.
        /// </summary>
        public bool UseInnerTransactionForBatches { get; set; }
        /// <summary>
        /// List of columns to include as parameters to the query from provided Source entities.
        /// All properties are included by default.
        /// </summary>
        public CommandArgs<TEntity> Source { get; }
        /// <summary>
        /// List of columns used to match Target table rows to Source rows.
        /// All properties are excluded by default.
        /// Parameter is required for all merge types except Insert. If not specified will insert all rows into Target table. 
        /// </summary>
        public MergeCompareArgs<TEntity> On { get; }
        /// <summary>
        /// Used if Update or Upsert type of merge is executed.
        /// List of columns to update on Target table for rows that did match Source rows.
        /// All properties are included by default.
        /// </summary>
        public MergeSetArgs<TEntity> SetMatched { get; }
        /// <summary>
        /// Used if Update type of merge is executed.
        /// List of columns to update on Target table for rows that did not match Source rows.
        /// All properties are excluded by default.
        /// </summary>
        public MergeSetArgs<TEntity> SetNotMatched { get; }
        /// <summary>
        /// Used if Insert or Upsert type of merge is executed.
        /// List of columns to insert.
        /// Database generated properties are excluded by default.
        /// All other properties are included by default.
        /// </summary>
        public MergeInsertArgs<TEntity> Insert { get; }
        /// <summary>
        /// List of properties to return for inserted rows. 
        /// Include properties that are generated on database side, like auto increment field.
        /// Returned values will be set to provided entities properties.
        /// Database generated or computed properties are included by default.
        /// Not implemented for TVP merge.
        /// </summary>
        public CommandArgs<TEntity> Output { get; }



        //methods
        string ConstructCommand(List<TEntity> entitiesBatch);
    }
}
