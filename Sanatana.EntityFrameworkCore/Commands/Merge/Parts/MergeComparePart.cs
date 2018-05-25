using Sanatana.EntityFrameworkCore.ColumnMapping;
using Sanatana.EntityFrameworkCore.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Commands.Merge
{
    public class MergeComparePart<TEntity> : MappingComponentBase<TEntity>
        where TEntity : class
    {
        //properties
        public List<Expression> Expressions { get; set; }


        //init
        public MergeComparePart(List<MappedProperty> allEntityProperties, MappedPropertyUtility mappedPropertyUtility)
            : base(allEntityProperties, mappedPropertyUtility)
        {
            Expressions = new List<Expression>();
        }


        //methods
        /// <summary>
        /// Expression to compare properties from Target and Source tables. Example (t, s) => t.IntProperty == s.OtherIntProperty или (t, s) => t.IntProperty == 5
        /// where t - target table, s - source table.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public MergeComparePart<TEntity> Condition<TKey>(Expression<Func<TEntity, TEntity, TKey>> condition)
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
        public virtual MergeComparePart<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Exclude property from list of compared properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeComparePart<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _excludePropertyEfDefaultNames.Add(propName);
            return this;
        }
    }
}
