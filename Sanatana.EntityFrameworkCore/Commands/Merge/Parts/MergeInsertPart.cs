using Sanatana.EntityFrameworkCore.ColumnMapping;
using Sanatana.EntityFrameworkCore.Expressions;
using Sanatana.EntityFrameworkCore.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Commands.Merge
{
    public class MergeInsertPart<TEntity> : MappingComponentBase<TEntity>
        where TEntity : class
    {
        //properties
        public Dictionary<string, string> Defaults { get; set; }


        //init
        public MergeInsertPart(List<MappedProperty> allEntityProperties, MappedPropertyUtility mappedPropertyUtility)
            : base(allEntityProperties, mappedPropertyUtility)
        {
            Defaults = new Dictionary<string, string>();
        }



        //methods
        /// <summary>
        /// Include property to the list of inserted properties and extract value from entity.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeInsertPart<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Exclude property from list of inserted properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeInsertPart<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _excludePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Include property to the list of inserted properties with predefined value.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public MergeInsertPart<TEntity> IncludeValue<TProp>(Expression<Func<TEntity, TProp>> property, TProp value)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            string sqlValue = ExpressionsToMSSql.ConstantToMSSql(value, typeof(TProp));
            Defaults[propName] = sqlValue;

            return this;
        }

        /// <summary>
        /// Include property to the list of inserted properties with empty value.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public MergeInsertPart<TEntity> IncludeDefaultValue<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            TProp value = default(TProp);
            string sqlValue = ExpressionsToMSSql.ConstantToMSSql(value, typeof(TProp));
            Defaults[propName] = sqlValue;

            return this;
        }

    }
}
