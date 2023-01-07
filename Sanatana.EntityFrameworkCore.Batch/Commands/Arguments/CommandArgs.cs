using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping
{
    public class CommandArgs<TEntity> : CommandArgsBase<TEntity>
        where TEntity : class
    {

        //init
        public CommandArgs(List<MappedProperty> allEntityProperties
            , PropertyMappingService propertyMappingService)
            : base(allEntityProperties, propertyMappingService)
        {
        }


        //methods
        /// <summary>
        /// Include property to the list of properties sent to database.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual CommandArgs<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionService.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Exclude property from the list of properties sent to database.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual CommandArgs<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionService.GetDefaultEfMemberName(property);
            _excludePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Set defaults for all columns when no property is selected explicitly.
        /// Will exclude all columns by default, except column sets provided as exceptColumns argument, that will be included.
        /// </summary>
        /// <param name="exceptColumns"></param>
        public virtual CommandArgs<TEntity> SetExcludeAllByDefault(params ColumnSetEnum[] exceptColumns)
        {
            ExcludeAllByDefault = true;

            if(exceptColumns == null || exceptColumns.Length == 0)
            {
                ExcludeDbGeneratedByDefault = ExcludeOptionsEnum.Exclude;
                ExcludePrimaryKeyByDefault = ExcludeOptionsEnum.Exclude;
                return this;
            }

            if (exceptColumns.Contains(ColumnSetEnum.DbGenerated))
            {
                ExcludeDbGeneratedByDefault = ExcludeOptionsEnum.Include;
            }
            if (exceptColumns.Contains(ColumnSetEnum.PrimaryKey))
            {
                ExcludePrimaryKeyByDefault = ExcludeOptionsEnum.Include;
            }

            return this;
        }

        /// <summary>
        /// Set defaults for all columns when no property is selected explicitly.
        /// Will include all columns by default, except column sets provided as exceptColumns argument, that will be excluded.
        /// </summary>
        /// <param name="exceptColumns"></param>
        public virtual CommandArgs<TEntity> SetIncludeAllByDefault(params ColumnSetEnum[] exceptColumns)
        {
            ExcludeAllByDefault = false;

            if (exceptColumns == null || exceptColumns.Length == 0)
            {
                ExcludeDbGeneratedByDefault = ExcludeOptionsEnum.Include;
                ExcludePrimaryKeyByDefault = ExcludeOptionsEnum.Include;
                return this;
            }

            if (exceptColumns.Contains(ColumnSetEnum.DbGenerated))
            {
                ExcludeDbGeneratedByDefault = ExcludeOptionsEnum.Exclude;
            }
            if (exceptColumns.Contains(ColumnSetEnum.PrimaryKey))
            {
                ExcludePrimaryKeyByDefault = ExcludeOptionsEnum.Exclude;
            }

            return this;
        }

        /// <summary>
        /// Clone CommandArgs from other CommandArgs instance.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual CommandArgs<TEntity> Clone(CommandArgsBase<TEntity> other)
        {
            ExcludeProperties = other.ExcludeProperties;
            IncludeProperties = other.IncludeProperties;
            ExcludeAllByDefault = other.ExcludeAllByDefault;
            ExcludeDbGeneratedByDefault = other.ExcludeDbGeneratedByDefault;

            return this;
        }
    }
}
