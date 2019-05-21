using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sanatana.EntityFrameworkCore.Batch.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;

namespace Sanatana.EntityFrameworkCore.Batch.ColumnMapping
{
    public class MappedPropertyUtility
    {
        //fields
        protected static Dictionary<string, List<MappedProperty>> _entityPropertyCache;
        protected DbContext _context;
        protected Type _rootEntityType;
        protected IEntityType _efRootEntityType;

        //init
        static MappedPropertyUtility()
        {
            _entityPropertyCache = new Dictionary<string, List<MappedProperty>>();
        }
        public MappedPropertyUtility(DbContext context, Type entityType)
        {
            _context = context;
            _rootEntityType = entityType;

            string rootEntityName = TypeExtensions.DisplayName(_rootEntityType);
            _efRootEntityType = _context.Model.FindEntityType(rootEntityName);
        }


        //Building property list
        public List<MappedProperty> GetAllEntityProperties()
        {
            if (_entityPropertyCache.ContainsKey(_rootEntityType.FullName) == false)
            {
                List<MappedProperty> properties = GetPropertiesHierarchy(_rootEntityType, new List<string>());
                _entityPropertyCache.Add(_rootEntityType.FullName, properties);
            }

            List<MappedProperty> cachedProperties = _entityPropertyCache[_rootEntityType.FullName];
            return CopyProperties(cachedProperties);
        }

        protected List<MappedProperty> GetPropertiesHierarchy(Type propertyType, List<string> parentPropertyNames)
        {
            if(parentPropertyNames.Count == 2)
            {
                throw new NotImplementedException("More than 1 level of Entity framework owned entities is not supported.");
            }

            List<MappedProperty> list = new List<MappedProperty>();
           
            //Include all primitive properties
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] allEntityProperties = propertyType.GetProperties(bindingFlags);
            List<PropertyInfo> primitiveProperties = allEntityProperties.Where(
                p =>
                {
                    Type underlying = Nullable.GetUnderlyingType(p.PropertyType);
                    Type property = underlying ?? p.PropertyType;
                    return property.IsValueType == true
                        || property.IsPrimitive
                        || property == typeof(string);
                  
                })
                .ToList();

            //Exclude ignored properties
            IEntityType efEntityType = null;
            if (propertyType == _rootEntityType)
            {
                efEntityType = _efRootEntityType;
            }
            else
            {
                string propertyName = parentPropertyNames[0];
                efEntityType = _context.GetOwnedProperty(_efRootEntityType, propertyName);
            }

            List<string> efMappedProperties = efEntityType.GetProperties()
                .Select(x => x.Name)
                .ToList();
            primitiveProperties = primitiveProperties
                .Where(x => efMappedProperties.Contains(x.Name))
                .ToList();

            //Return all the rest properties
            foreach (PropertyInfo supportedProp in primitiveProperties)
            {
                List<string> namesHierarchy = parentPropertyNames.ToList();
                namesHierarchy.Add(supportedProp.Name);

                string efDefaultName = ReflectionUtility.ConcatenateEfPropertyName(namesHierarchy);
                IProperty property = DbContextExtensions.GetPropertyMapping(_context, _rootEntityType, efDefaultName);
                list.Add(new MappedProperty
                {
                    PropertyInfo = supportedProp,
                    EfDefaultName = efDefaultName,
                    EfMappedName = property.Relational().ColumnName,
                    ConfiguredSqlType = property.GetConfiguredColumnType()
                });
            }

            //Add complex properties
            List<MappedProperty> complexProperties = GetComplexProperties(allEntityProperties, parentPropertyNames);
            list.AddRange(complexProperties);
            return list;
        }

        protected List<MappedProperty> GetComplexProperties(PropertyInfo[] allEntityProperties, List<string> parentPropertyNames)
        {
            List<MappedProperty> list = new List<MappedProperty>();

            //Include only classes, except strings
            List<PropertyInfo> complexProperties = allEntityProperties.Where(
                p => p.PropertyType != typeof(string)
                && p.PropertyType.IsClass)
                .ToList();

            //Exclude navigation properties
            //ICollection properties are excluded already as interfaces and not classes.
            //Here we exclude virtual properties that are not ICollection, by checking if they are virtual.
            complexProperties = complexProperties
                .Where(x => x.GetAccessors()[0].IsVirtual == false)
                .ToList();
            
            //Exclude ignored properties
            List<string> mappedComplexTypes = _context.Model.GetEntityTypes()
                .Where(x => x.IsOwned())
                .Select(x => x.Name)
                .ToList();
            complexProperties = complexProperties
                .Where(x => mappedComplexTypes.Contains(x.PropertyType.FullName))
                .ToList();

            //Return all the rest complex properties
            foreach (PropertyInfo complexProp in complexProperties)
            {
                List<string> namesHierarchy = parentPropertyNames.ToList();
                namesHierarchy.Add(complexProp.Name);

                List<MappedProperty> childProperties = GetPropertiesHierarchy(complexProp.PropertyType, namesHierarchy);
                if (childProperties.Count > 0)
                {
                    list.Add(new MappedProperty
                    {
                        PropertyInfo = complexProp,
                        ChildProperties = childProperties
                    });
                }
            }

            return list;
        }

        protected List<MappedProperty> CopyProperties(List<MappedProperty> properties)
        {
            List<MappedProperty> list = new List<MappedProperty>();

            foreach (MappedProperty prop in properties)
            {
                MappedProperty copy = prop.Copy();
                if (prop.IsComplexProperty)
                {
                    copy.ChildProperties = CopyProperties(prop.ChildProperties);
                }
                list.Add(copy);
            }

            return list;
        }


        //Property values
        public void GetValues(List<MappedProperty> properties, object entity)
        {
            foreach (MappedProperty property in properties)
            {
                property.Value = property.PropertyInfo.GetValue(entity);
                if (property.PropertyInfo.PropertyType.IsEnum)
                {
                    property.Value = (int)property.Value;
                }

                if (property.IsComplexProperty)
                {
                    GetValues(property.ChildProperties, property.Value);
                }
            }
        }


        //Filtering and ordering
        public List<MappedProperty> FilterProperties(List<MappedProperty> properties, bool hasOtherConditions
            , List<string> includeProperties, List<string> excludeProperties
            , bool excludeAllByDefault, ExcludeOptions includeDbGeneratedProperties)
        {
            List<MappedProperty> selectedProperties;

            if (includeProperties.Count > 0 || hasOtherConditions)
            {
                selectedProperties = properties.Where(
                    pr => pr.IsComplexProperty
                    || includeProperties.Contains(pr.EfDefaultName))
                   .ToList();
            }
            else if (excludeProperties.Count > 0)
            {
                selectedProperties = properties.Where(
                    pr => pr.IsComplexProperty
                    || !excludeProperties.Contains(pr.EfDefaultName))
                   .ToList();
            }
            else if (excludeAllByDefault)
            {
                selectedProperties = new List<MappedProperty>();
                
                if (includeDbGeneratedProperties == ExcludeOptions.Include)
                {
                    List<string> generatedProps = _context.GetDatabaseGeneratedProperties(_rootEntityType);
                    selectedProperties = properties
                        .Where(x => generatedProps.Contains(x.EfMappedName))
                        .ToList();
                }
            }
            else //include all by default
            {
                selectedProperties = properties;
                
                if (includeDbGeneratedProperties == ExcludeOptions.Exclude)
                {
                    List<string> generatedProps = _context.GetDatabaseGeneratedProperties(_rootEntityType);
                    selectedProperties = selectedProperties
                        .Where(x => !generatedProps.Contains(x.EfMappedName))
                        .ToList();
                }
            }

            //exclude properties that are not mapped to any column
            selectedProperties = selectedProperties.Where(
                pr => pr.IsComplexProperty 
                || pr.EfMappedName != null)
                .ToList();

            for (int i = 0; i < selectedProperties.Count; i++)
            {
                if (selectedProperties[i].IsComplexProperty)
                {
                    MappedProperty copy = selectedProperties[i].Copy();
                    copy.ChildProperties = FilterProperties(selectedProperties[i].ChildProperties, hasOtherConditions
                        , includeProperties, excludeProperties, excludeAllByDefault, includeDbGeneratedProperties);
                    selectedProperties[i] = copy;
                }
            }

            return selectedProperties;
        }

        public List<MappedProperty> FlattenHierarchy(List<MappedProperty> properties)
        {
            List<MappedProperty> selectedProperties = new List<MappedProperty>();

            foreach (MappedProperty item in properties)
            {
                if (item.IsComplexProperty)
                {
                    List<MappedProperty> children = FlattenHierarchy(item.ChildProperties);
                    selectedProperties.AddRange(children);
                }
                else
                {
                    selectedProperties.Add(item);
                }
            }

            return selectedProperties;
        }

        public List<MappedProperty> OrderFlatBySelectedProperties(List<MappedProperty> properties, List<string> includeProperties)
        {
            if(includeProperties.Count == 0)
            {
                return properties;
            }

            List<MappedProperty> result = new List<MappedProperty>();

            foreach (string includedPropName in includeProperties)
            {
                MappedProperty mappedProperty = properties.First(x => x.EfDefaultName == includedPropName);
                result.Add(mappedProperty);
            }

            return result;
        }
    }
}
