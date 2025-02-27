using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Commands.Arguments;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Internals.Reflection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Internal = Microsoft.EntityFrameworkCore.Internal;


namespace Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping
{
    public class PropertyMappingService
    {
        //fields
        protected static Dictionary<string, List<MappedProperty>> _entityPropertyCache;
        protected DbContext _dbContext;
        protected Type _rootEntityType;
        protected IEntityType _efRootEntityType;
        protected IDbParametersService _dbParametersService;

        //init
        static PropertyMappingService()
        {
            _entityPropertyCache = new Dictionary<string, List<MappedProperty>>();
        }

        public PropertyMappingService(DbContext context, Type entityType, IDbParametersService dbParametersService)
        {
            _dbContext = context;
            _rootEntityType = entityType;
            _efRootEntityType = _dbContext.Model.FindEntityType(_rootEntityType);
            _dbParametersService = dbParametersService;
        }


        //Building property list
        public virtual List<MappedProperty> GetAllEntityProperties()
        {
            if (_entityPropertyCache.ContainsKey(_rootEntityType.FullName) == false)
            {
                List<MappedProperty> properties = GetPropertiesHierarchy(_rootEntityType, new List<string>());
                _entityPropertyCache.Add(_rootEntityType.FullName, properties);
            }

            List<MappedProperty> cachedProperties = _entityPropertyCache[_rootEntityType.FullName];
            return CopyProperties(cachedProperties);
        }

        protected virtual List<MappedProperty> GetPropertiesHierarchy(Type propertyType, List<string> parentPropertyNames)
        {
            if (parentPropertyNames.Count == 2)
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
                efEntityType = _dbContext.GetOwnedProperty(_efRootEntityType, propertyName);
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

                string efDefaultName = ReflectionService.ConcatenateEfPropertyName(namesHierarchy);
                IProperty property = _dbContext.GetPropertyMapping(_rootEntityType, efDefaultName);
                list.Add(new MappedProperty
                {
                    PropertyInfo = supportedProp,
                    PocoPropertyName = efDefaultName,
                    DbColumnName = property.GetStoreObjectIdColumnName(),
                    ConfiguredSqlType = property.GetColumnType()
                });
            }

            //Add complex properties
            List<MappedProperty> complexProperties = GetComplexProperties(allEntityProperties, parentPropertyNames);
            list.AddRange(complexProperties);
            return list;
        }

        protected virtual List<MappedProperty> GetComplexProperties(PropertyInfo[] allEntityProperties, List<string> parentPropertyNames)
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
            List<string> mappedComplexTypes = _dbContext.Model.GetEntityTypes()
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

        protected virtual List<MappedProperty> CopyProperties(List<MappedProperty> properties)
        {
            List<MappedProperty> list = new List<MappedProperty>();

            foreach (MappedProperty prop in properties)
            {
                MappedProperty copy = prop.Clone();
                if (prop.IsComplexProperty)
                {
                    copy.ChildProperties = CopyProperties(prop.ChildProperties);
                }
                list.Add(copy);
            }

            return list;
        }


        //Property values methods
        public virtual void GetValues(List<MappedProperty> properties, object entity)
        {
            foreach (MappedProperty property in properties)
            {
                if (entity == null)
                {
                    // entity can be null if there are no ChildProperties, calling GetValues(property.ChildProperties, property.Value);
                    property.Value = null;
                    continue;
                }

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


        //Filtering and ordering methods
        public virtual List<MappedProperty> FilterProperties(List<MappedProperty> properties, bool hasOtherConditions
            , ICommandArgs commandArgs)
        {
            List<MappedProperty> selectedProperties = new List<MappedProperty>();

            List<string> includeProperties = commandArgs.IncludeProperties;
            List<string> excludeProperties = commandArgs.ExcludeProperties;
            bool excludeAllByDefault = commandArgs.ExcludeAllByDefault;
            ExcludeOptionsEnum excludeDbGeneratedProperties = commandArgs.ExcludeDbGeneratedByDefault;
            ExcludeOptionsEnum excludePrimaryKeyByDefault = commandArgs.ExcludePrimaryKeyByDefault;
            bool useExplicitColumns = includeProperties.Count > 0 || excludeProperties.Count > 0 || hasOtherConditions;

            if (useExplicitColumns)
            {
                if (includeProperties.Count > 0 || hasOtherConditions)
                {
                    selectedProperties = properties.Where(
                        pr => pr.IsComplexProperty
                        || includeProperties.Contains(pr.PocoPropertyName))
                       .ToList();
                }
                else if (excludeProperties.Count > 0)
                {
                    selectedProperties = properties.Where(
                        pr => pr.IsComplexProperty
                        || !excludeProperties.Contains(pr.PocoPropertyName))
                       .ToList();
                }
            }
            else // use defaults
            {
                if (excludeAllByDefault)
                {
                    selectedProperties = new List<MappedProperty>();

                    if (excludeDbGeneratedProperties == ExcludeOptionsEnum.Include)
                    {
                        string[] generatedCols = _dbContext.GetDatabaseGeneratedColumns(_rootEntityType);
                        MappedProperty[] generatedMappedProps = properties
                            .Where(x => generatedCols.Contains(x.DbColumnName))
                            .ToArray();
                        selectedProperties = selectedProperties.ConcatDistinct(generatedMappedProps);
                    }
                    if (excludePrimaryKeyByDefault == ExcludeOptionsEnum.Include)
                    {
                        string[] primaryCols = _dbContext.GetPrimaryKeyColumns(_rootEntityType);
                        MappedProperty[] primaryMappedProps = properties
                            .Where(x => primaryCols.Contains(x.DbColumnName))
                            .ToArray();
                        selectedProperties = selectedProperties.ConcatDistinct(primaryMappedProps);
                    }
                }
                else //include all by default
                {
                    selectedProperties = properties;

                    if (excludeDbGeneratedProperties == ExcludeOptionsEnum.Exclude)
                    {
                        string[] generatedCols = _dbContext.GetDatabaseGeneratedColumns(_rootEntityType);
                        selectedProperties = selectedProperties
                            .Where(x => !generatedCols.Contains(x.DbColumnName))
                            .ToList();
                    }
                    if (excludePrimaryKeyByDefault == ExcludeOptionsEnum.Exclude)
                    {
                        string[] primaryCols = _dbContext.GetPrimaryKeyColumns(_rootEntityType);
                        selectedProperties = selectedProperties
                            .Where(x => !primaryCols.Contains(x.DbColumnName))
                            .ToList();
                    }
                }
            }

            //exclude properties that are not mapped to any column
            selectedProperties = selectedProperties.Where(
                pr => pr.IsComplexProperty
                || pr.DbColumnName != null)
                .ToList();

            for (int i = 0; i < selectedProperties.Count; i++)
            {
                if (selectedProperties[i].IsComplexProperty)
                {
                    MappedProperty clone = selectedProperties[i].Clone();
                    clone.ChildProperties = FilterProperties(selectedProperties[i].ChildProperties, hasOtherConditions, commandArgs);
                    selectedProperties[i] = clone;
                }
            }

            return selectedProperties;
        }

        public virtual List<MappedProperty> FlattenHierarchy(List<MappedProperty> properties)
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

        public virtual List<MappedProperty> OrderFlatBySelectedProperties(List<MappedProperty> properties, List<string> includeProperties)
        {
            if (includeProperties.Count == 0)
            {
                return properties;
            }

            List<MappedProperty> result = new List<MappedProperty>();

            foreach (string includedPropName in includeProperties)
            {
                MappedProperty mappedProperty = properties.First(x => x.PocoPropertyName == includedPropName);
                result.Add(mappedProperty);
            }

            return result;
        }


        //Combine methods
        public virtual string CombineColumns<TEntity>(CommandArgs<TEntity> commandArgs, string description) 
            where TEntity : class
        {
            List<MappedProperty> selectedProperties = commandArgs.GetSelectedFlat();

            string[] columnNames = selectedProperties
                .Select(x => _dbParametersService.FormatColumnName(x.DbColumnName))
                .ToArray();

            if (columnNames.Length == 0)
            {
                throw new NotSupportedException($"Selected 0 columns for entity {typeof(TEntity).FullName} in {description} part of the query.");
            }

            return string.Join(",", columnNames);
        }

        public virtual string CombineInsertValues<TEntity>(CommandArgs<TEntity> commandArgs, List<TEntity> entities, out DbParameter[] parameters)
               where TEntity : class
        {
            List<DbParameter> sqlParams = new List<DbParameter>();
            StringBuilder commandText = new StringBuilder();

            for (int i = 0; i < entities.Count; i++)
            {
                commandText.Append("(");

                TEntity entity = entities[i];
                List<MappedProperty> entityProperties = commandArgs.GetSelectedFlatWithValues(entity);

                for (int p = 0; p < entityProperties.Count; p++)
                {
                    if (entityProperties[p].Value == null)
                    {
                        commandText.Append("NULL");
                    }
                    else
                    {
                        string paramName = $"{entityProperties[p].DbColumnName}{i}";
                        paramName = _dbParametersService.FormatParameterName(paramName);
                        commandText.Append(paramName);

                        DbParameter dbParameter = entityProperties[p].ConfiguredSqlType == null
                            ? _dbParametersService.GetDbParameter(paramName, entityProperties[p].Value)
                            : _dbParametersService.GetDbParameter(paramName, entityProperties[p].Value, entityProperties[p]);
                        sqlParams.Add(dbParameter);
                    }

                    bool isLastProperty = p == entityProperties.Count - 1;
                    if (isLastProperty == false)
                    {
                        commandText.Append(",");
                    }
                }

                commandText.Append(")");
                bool isLastEntity = i == entities.Count - 1;
                if (isLastEntity == false)
                {
                    commandText.Append(",");
                }
            }

            parameters = sqlParams.ToArray();
            return commandText.ToString();
        }

        public virtual string CombineOutput<TEntity>(CommandArgs<TEntity> outputArgs, string? alias = null)
            where TEntity : class
        {
            List<MappedProperty> outputProperties = outputArgs.GetSelectedFlat();
            if (outputProperties.Count == 0)
            {
                return "";
            }

            alias = alias == null ? null : $"{alias}.";

            StringBuilder text = new StringBuilder();
            for (int i = 0; i < outputProperties.Count; i++)
            {
                MappedProperty prop = outputProperties[i];
                string outputParameter = $"{alias}{_dbParametersService.FormatOutputParameter(prop.DbColumnName)}";
                text.Append(outputParameter);

                bool isLast = i == outputProperties.Count - 1;
                if (!isLast)
                {
                    text.Append(",");
                }
            }

            return text.ToString();
        }

        public virtual string CombineSet<TEntity>(List<Expression> setExpressions, bool useLambdaAlias = false)
            where TEntity : class
        {
            if (setExpressions == null || setExpressions.Count == 0)
            {
                throw new ArgumentNullException($"Provided {nameof(setExpressions)} is empty. Use {nameof(UpdateCommand<TEntity>.SetAssign)} method to set.");
            }

            List<string> setStrings = new List<string>();
            foreach (Expression item in setExpressions)
            {
                string expressionSql = item.ToSqlString(_dbParametersService, _dbContext, useLambdaAlias: false);
                setStrings.Add(expressionSql);
            }
            string setPart = string.Join(", ", setStrings);
            return _dbParametersService.FormatExpression(setPart);
        }

        public virtual string CombineSetFromValues<TEntity>(MergeSetArgs<TEntity> setArgs, string targetAlias, string sourceAlias, bool useLeftAlias = true)
            where TEntity : class
        {
            string? targetAliasDot = useLeftAlias ? $"{targetAlias}." : null;

            List<string> updateParts = setArgs.GetSelectedFlat()
                .Select(p => _dbParametersService.FormatColumnName(p.DbColumnName))
                .Select(efMappedName => string.Format("{0}{1}={2}.{1}", targetAliasDot, efMappedName, sourceAlias))
                .Select(exp => _dbParametersService.FormatExpression(exp))
                .ToList();

            string? targetLeftAlias = useLeftAlias ? targetAlias : null;
            string?[] leftAliases = new string?[] { targetLeftAlias, sourceAlias };
            string[] rightAliases = new string[] { targetAlias, sourceAlias };
            foreach (AssignLambdaExpression item in setArgs.Expressions)
            {
                string expressionLeftSql = item.Left.ToSqlString(_dbParametersService, _dbContext, alternativeAliases: leftAliases);
                string expressionRightSql = item.Right.ToSqlString(_dbParametersService, _dbContext, alternativeAliases: rightAliases);
                string expressionSql = $"{expressionLeftSql} = {expressionRightSql}";
                updateParts.Add(_dbParametersService.FormatExpression(expressionSql));
            }

            return string.Join(", ", updateParts);
        }

        public virtual string CombineWhere<TEntity>(Expression<Func<TEntity, bool>> whereExpression, bool useLambdaAlias = false)
            where TEntity : class
        {
            if (whereExpression == null)
            {
                throw new ArgumentNullException($"Provided {nameof(whereExpression)} is null. Use {nameof(UpdateCommand<TEntity>.SetWhere)} method to set.");
            }

            string matchPart = null;
            bool? booleanBody = ExpressionsToSql.TryGetBooleanBody(whereExpression, _dbParametersService);
            if (booleanBody == null)
            {
                matchPart = whereExpression.ToSqlString(_dbParametersService, _dbContext, useLambdaAlias);
            }
            else
            {
                string dbBool = _dbParametersService.FormatBoolean(true);
                matchPart = booleanBody.Value
                    ? $"{dbBool} = {dbBool}"
                    : $"{dbBool} <> {dbBool}";
            }

            return _dbParametersService.FormatExpression(matchPart);
        }

        public virtual string CombineColumns<TEntity>(MergeInsertArgs<TEntity> commandArgs)
            where TEntity : class
        {
            List<string> columnNames = new List<string>();

            List<MappedProperty> selectedProperties = commandArgs.GetSelectedFlat();
            foreach (MappedProperty selectedProperty in selectedProperties)
            {
                if (commandArgs.Defaults.ContainsKey(selectedProperty.PocoPropertyName) == false)
                {
                    columnNames.Add(_dbParametersService.FormatColumnName(selectedProperty.DbColumnName));
                }
            }

            List<MappedProperty> allProperties = GetAllEntityProperties();
            foreach (MappedProperty entityProperty in allProperties)
            {
                if (commandArgs.Defaults.ContainsKey(entityProperty.PocoPropertyName))
                {
                    columnNames.Add(_dbParametersService.FormatColumnName(entityProperty.DbColumnName));
                }
            }

            return string.Join(",", columnNames);
        }

        public virtual string CombineMergeInsertValues<TEntity>(MergeInsertArgs<TEntity> commandArgs, string alias)
            where TEntity : class
        {
            List<string> values = new List<string>();

            List<MappedProperty> selectedProperties = commandArgs.GetSelectedFlat();
            foreach (MappedProperty selectedProperty in selectedProperties)
            {
                if (commandArgs.Defaults.ContainsKey(selectedProperty.PocoPropertyName) == false)
                {
                    values.Add(alias + "." + selectedProperty.DbColumnName);
                }
            }

            List<MappedProperty> allProperties = GetAllEntityProperties();
            foreach (MappedProperty entityProperty in allProperties)
            {
                if (commandArgs.Defaults.ContainsKey(entityProperty.PocoPropertyName))
                {
                    values.Add(commandArgs.Defaults[entityProperty.PocoPropertyName]);
                }
            }

            return string.Join(", ", values);
        }
    }
}
