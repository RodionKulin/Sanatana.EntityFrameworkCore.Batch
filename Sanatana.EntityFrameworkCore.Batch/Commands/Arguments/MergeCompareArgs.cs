using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Commands.Arguments
{
    public class MergeCompareArgs<TEntity> : CommandArgsBase<TEntity>
        where TEntity : class
    {
        //properties
        public List<Expression> Expressions { get; set; }

        protected override bool HasOtherConditions
        {
            get
            {
                return Expressions.Count > 0;
            }
        }



        //init
        public MergeCompareArgs(List<MappedProperty> allEntityProperties, PropertyMappingService propertyMappingService)
            : base(allEntityProperties, propertyMappingService)
        {
            Expressions = new List<Expression>();
        }


        //methods
        /// <summary>
        /// Expression to compare properties from Target and Source tables. Example (t, s) => t.IntProperty == s.OtherIntProperty или (t, s) => t.IntProperty == 5
        /// where t - target table, s - source table.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public MergeCompareArgs<TEntity> Condition<TKey>(Expression<Func<TEntity, TEntity, TKey>> condition)
        {
            Expressions.Add(condition);
            return this;
        }

        /// <summary>
        /// Include property to the list of compared properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeCompareArgs<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionService.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Exclude property from list of compared properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeCompareArgs<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
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
        public virtual MergeCompareArgs<TEntity> SetExcludeAllByDefault(params ColumnSetEnum[] exceptColumns)
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
        public virtual MergeCompareArgs<TEntity> SetIncludeAllByDefault(params ColumnSetEnum[] exceptColumns)
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
        /// Clone MergeCompareArgs from other MergeCompareArgs instance.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual MergeCompareArgs<TEntity> Clone(MergeCompareArgs<TEntity> other)
        {
            Expressions = new List<Expression>(other.Expressions);
            ExcludeProperties = other.ExcludeProperties;
            IncludeProperties = other.IncludeProperties;
            ExcludeAllByDefault = other.ExcludeAllByDefault;
            ExcludeDbGeneratedByDefault = other.ExcludeDbGeneratedByDefault;

            return this;
        }
    }
}
