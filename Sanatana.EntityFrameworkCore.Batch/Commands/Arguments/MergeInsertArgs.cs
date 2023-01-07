using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch.Internals;

namespace Sanatana.EntityFrameworkCore.Batch.Commands.Arguments
{
    public class MergeInsertArgs<TEntity> : CommandArgsBase<TEntity>
        where TEntity : class
    {
        //fields
        protected IDbParametersService _dbParametersService;


        //properties
        /// <summary>
        /// Constant column values to insert for all entities provided.
        /// </summary>
        public Dictionary<string, string> Defaults { get; set; }

        protected override bool HasOtherConditions
        {
            get
            {
                return Defaults.Count > 0;
            }
        }


        //init
        public MergeInsertArgs(List<MappedProperty> allEntityProperties, PropertyMappingService propertyMappingService,
            IDbParametersService dbParametersService)
            : base(allEntityProperties, propertyMappingService)
        {
            Defaults = new Dictionary<string, string>();

            _dbParametersService = dbParametersService;
        }



        //methods
        /// <summary>
        /// Include property to the list of inserted properties and extract value from entity.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeInsertArgs<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionService.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Exclude property from list of inserted properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeInsertArgs<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionService.GetDefaultEfMemberName(property);
            _excludePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Include property to the list of inserted properties with constant value.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeInsertArgs<TEntity> IncludeValue<TProp>(Expression<Func<TEntity, TProp>> property, TProp value)
        {
            string propName = ReflectionService.GetDefaultEfMemberName(property);
            string sqlValue = ExpressionsToSql.ConstantToSql(value, typeof(TProp), _dbParametersService);
            Defaults[propName] = sqlValue;

            return this;
        }

        /// <summary>
        /// Include property to the list of inserted properties with empty value.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeInsertArgs<TEntity> IncludeDefaultValue<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionService.GetDefaultEfMemberName(property);
            TProp value = default;
            string sqlValue = ExpressionsToSql.ConstantToSql(value, typeof(TProp), _dbParametersService);
            Defaults[propName] = sqlValue;

            return this;
        }

        /// <summary>
        /// Set defaults for all columns when no property is selected explicitly.
        /// Will exclude all columns by default, except column sets provided as exceptColumns argument, that will be included.
        /// </summary>
        /// <param name="exceptColumns"></param>
        public virtual MergeInsertArgs<TEntity> SetExcludeAllByDefault(params ColumnSetEnum[] exceptColumns)
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
        public virtual MergeInsertArgs<TEntity> SetIncludeAllByDefault(params ColumnSetEnum[] exceptColumns)
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


    }
}
