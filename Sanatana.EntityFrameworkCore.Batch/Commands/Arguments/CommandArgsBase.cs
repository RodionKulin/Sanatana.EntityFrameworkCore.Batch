using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping
{
    public abstract class CommandArgsBase<TEntity> : ICommandArgs
        where TEntity : class
    {
        //fields
        protected List<MappedProperty> _allEntityProperties;
        protected List<string> _includePropertyEfDefaultNames;
        protected List<string> _excludePropertyEfDefaultNames;
        protected PropertyMappingService _propertyMappingService;
        protected bool _hasOtherConditions;

        
        //protected properties
        protected virtual bool HasOtherConditions
        {
            get { return _hasOtherConditions; }
        }


        //properties
        /// <summary>
        /// Explicitly included properties to the list of properties sent to database.
        /// </summary>
        public List<string> IncludeProperties 
        { 
            get { return _includePropertyEfDefaultNames; } 
            protected set { _includePropertyEfDefaultNames = value; }
        }

        /// <summary>
        /// Explicitly excluded properties from the list of properties sent to database.
        /// </summary>
        /// <returns></returns>
        public List<string> ExcludeProperties
        {
            get { return _excludePropertyEfDefaultNames; }
            protected set { _excludePropertyEfDefaultNames = value; }
        }

        /// <summary>
        /// Set default to exclude all properties if no properties are included, excluded or assigned exclicitly.
        /// </summary>
        public bool ExcludeAllByDefault { get; protected set; }

        /// <summary>
        /// Set default to exclude database generated properties if no properties are included, excluded or assigned exclicitly.
        /// </summary>
        public ExcludeOptionsEnum ExcludeDbGeneratedByDefault { get; protected set; }

        /// <summary>
        /// Set default to exclude primary key properties if no properties are included, excluded or assigned exclicitly.
        /// </summary>
        public ExcludeOptionsEnum ExcludePrimaryKeyByDefault { get; protected set; }


        //init
        public CommandArgsBase(List<MappedProperty> allEntityProperties
            , PropertyMappingService propertyMappingService)
        {
            _allEntityProperties = allEntityProperties;
            _propertyMappingService = propertyMappingService;

            _includePropertyEfDefaultNames = new List<string>();
            _excludePropertyEfDefaultNames = new List<string>();
        }


        //methods
        public virtual List<MappedProperty> GetSelectedFlat()
        {
            List<MappedProperty> selected = _propertyMappingService.FilterProperties(_allEntityProperties, HasOtherConditions, this);

            selected = _propertyMappingService.FlattenHierarchy(selected);
            selected = _propertyMappingService.OrderFlatBySelectedProperties(selected, _includePropertyEfDefaultNames);
            return selected;
        }

        public virtual List<MappedProperty> GetSelectedFlatWithValues(object entity)
        {
            List<MappedProperty> selected = _propertyMappingService.FilterProperties(_allEntityProperties, HasOtherConditions, this);

            _propertyMappingService.GetValues(selected, entity);
            selected = _propertyMappingService.FlattenHierarchy(selected);
            selected = _propertyMappingService.OrderFlatBySelectedProperties(selected, _includePropertyEfDefaultNames);

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
