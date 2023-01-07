using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Commands.Arguments
{
    public class MergeSetArgs<TEntity> : CommandArgsBase<TEntity>
        where TEntity : class
    {
        //properties
        public List<AssignLambdaExpression> Expressions { get; set; }

        protected override bool HasOtherConditions
        {
            get
            {
                return Expressions.Count > 0;
            }
        }


        //init
        public MergeSetArgs(List<MappedProperty> allEntityProperties, PropertyMappingService propertyMappingService)
            : base(allEntityProperties, propertyMappingService)
        {
            Expressions = new List<AssignLambdaExpression>();
        }



        //methods
        /// <summary>
        /// Expression to update columns of Target table. Example: (t) => t.IntProperty, (t, s) => s.OtherIntProperty * 2
        /// where t - target table, s - source table.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="targetProperty"></param>
        /// <param name="assignedValue"></param>
        /// <returns></returns>
        public MergeSetArgs<TEntity> Assign<TProp>(Expression<Func<TEntity, TProp>> targetProperty
            , Expression<Func<TEntity, TEntity, TProp>> assignedValue)
        {
            Expressions.Add(new AssignLambdaExpression()
            {
                Left = targetProperty,
                Right = assignedValue
            });
            return this;
        }

        /// <summary>
        /// Include property to the list of updated properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeSetArgs<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionService.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Exclude property from list of updated properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeSetArgs<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
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
        public virtual MergeSetArgs<TEntity> SetExcludeAllByDefault(params ColumnSetEnum[] exceptColumns)
        {
            ExcludeAllByDefault = true;

            if (exceptColumns == null || exceptColumns.Length == 0)
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
        public virtual MergeSetArgs<TEntity> SetIncludeAllByDefault(params ColumnSetEnum[] exceptColumns)
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



        //internal methods
        /// <summary>
        /// Internal method to clone MergeUpdateArgs from other MergeUpdateArgs instance.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual MergeSetArgs<TEntity> Clone(MergeSetArgs<TEntity> other)
        {
            ExcludeProperties = other.ExcludeProperties;
            IncludeProperties = other.IncludeProperties;
            ExcludeAllByDefault = other.ExcludeAllByDefault;
            ExcludeDbGeneratedByDefault = other.ExcludeDbGeneratedByDefault;
            Expressions = other.Expressions;

            return this;
        }

    }
}
