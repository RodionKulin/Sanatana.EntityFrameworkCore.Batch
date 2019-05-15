using Sanatana.EntityFrameworkCore.Batch.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.ColumnMapping
{
    public class CommandArgs<TEntity> : CommandArgsBase<TEntity>
        where TEntity : class
    {

        //init
        public CommandArgs(List<MappedProperty> allEntityProperties
               , MappedPropertyUtility mappedPropertyUtility)
            : base(allEntityProperties, mappedPropertyUtility)
        {
        }


        //methods
        /// <summary>
        /// Include property to the list of properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual CommandArgs<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        public virtual CommandArgs<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _excludePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Set defaults when no property is selected.
        /// </summary>
        /// <param name="excludeAllByDefault"></param>
        public virtual CommandArgs<TEntity> SetExcludeAllByDefault(bool excludeAllByDefault)
        {
            ExcludeAllByDefault = excludeAllByDefault;
            return this;
        }

        /// <summary>
        /// Set defaults for database generated properties when no property is selected.
        /// </summary>
        /// <param name="excludeDbGeneratedByDefault"></param>
        /// <returns></returns>
        public virtual CommandArgs<TEntity> SetExcludeDbGeneratedByDefault(
            ExcludeOptions excludeDbGeneratedByDefault)
        {
            ExcludeDbGeneratedByDefault = excludeDbGeneratedByDefault;
            return this;
        }

    }
}
