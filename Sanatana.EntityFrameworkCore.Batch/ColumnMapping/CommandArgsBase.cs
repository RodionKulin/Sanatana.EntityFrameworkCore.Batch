using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.ColumnMapping
{
    public abstract class CommandArgsBase<TEntity>
        where TEntity : class
    {
        //fields
        protected List<MappedProperty> _allEntityProperties;
        protected List<string> _includePropertyEfDefaultNames;
        protected List<string> _excludePropertyEfDefaultNames;
        protected MappedPropertyUtility _mappedPropertyUtility;
        protected bool _hasOtherConditions;


        //properties
        /// <summary>
        /// Set default to include all properties if no additional configuration provided. 
        /// If no properties are included, excluded or assigned.
        /// </summary>
        public bool ExcludeAllByDefault { get; set; }

        /// <summary>
        /// Set default to include database generated properties if no additional configuration provided. 
        /// If no properties are included, excluded or assigned.
        /// </summary>
        public ExcludeOptions ExcludeDbGeneratedByDefault { get; set; }


        //init
        public CommandArgsBase(List<MappedProperty> allEntityProperties
            , MappedPropertyUtility mappedPropertyUtility)
        {
            _allEntityProperties = allEntityProperties;
            _mappedPropertyUtility = mappedPropertyUtility;

            _includePropertyEfDefaultNames = new List<string>();
            _excludePropertyEfDefaultNames = new List<string>();
        }


        //methods
        public virtual List<MappedProperty> GetSelectedFlat()
        {
            List<MappedProperty> selected = _mappedPropertyUtility.FilterProperties(_allEntityProperties, _hasOtherConditions
                , _includePropertyEfDefaultNames, _excludePropertyEfDefaultNames, ExcludeAllByDefault, ExcludeDbGeneratedByDefault);

            selected = _mappedPropertyUtility.FlattenHierarchy(selected);
            selected = _mappedPropertyUtility.OrderFlatBySelectedProperties(selected, _includePropertyEfDefaultNames);
            return selected;
        }

        public virtual List<MappedProperty> GetSelectedFlatWithValues(object entity)
        {
            List<MappedProperty> selected = _mappedPropertyUtility.FilterProperties(_allEntityProperties, _hasOtherConditions
                , _includePropertyEfDefaultNames, _excludePropertyEfDefaultNames, ExcludeAllByDefault, ExcludeDbGeneratedByDefault);

            _mappedPropertyUtility.GetValues(selected, entity);
            selected = _mappedPropertyUtility.FlattenHierarchy(selected);
            selected = _mappedPropertyUtility.OrderFlatBySelectedProperties(selected, _includePropertyEfDefaultNames);

            return selected;
        }

        internal virtual List<string> GetSelectedPropertyNames()
        {
            return GetSelectedFlat()
                .Select(p => p.PropertyInfo.Name)
                .ToList();
        }
    }
}
