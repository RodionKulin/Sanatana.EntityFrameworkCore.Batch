using Sanatana.EntityFrameworkCore.ColumnMapping;
using Sanatana.EntityFrameworkCore.Expressions;
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
    public class MergeUpdatePart<TEntity> : MappingComponentBase<TEntity>
        where TEntity : class
    {
        //properties
        public List<Expression> Expressions { get; set; }


        //init
        public MergeUpdatePart(List<MappedProperty> allEntityProperties, MappedPropertyUtility mappedPropertyUtility)
            : base(allEntityProperties, mappedPropertyUtility)
        {
            Expressions = new List<Expression>();
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
        public MergeUpdatePart<TEntity> Assign<TProp>(Expression<Func<TEntity, TProp>> targetProperty
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
        public virtual MergeUpdatePart<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        /// <summary>
        /// Exclude property from list of updated properties.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual MergeUpdatePart<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _excludePropertyEfDefaultNames.Add(propName);
            return this;
        }
    }
}
