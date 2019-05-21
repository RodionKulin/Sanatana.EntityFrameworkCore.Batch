using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Sanatana.EntityFrameworkCore.Batch.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Get schema and name of the table used by EF.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetTableName<T>(this DbContext context)
            where T : class
        {
            string entityName = TypeExtensions.DisplayName(typeof(T));
            IEntityType rootEntityType = context.Model.FindEntityType(entityName);
            if(rootEntityType == null)
            {
                throw new KeyNotFoundException($"Entity type {entityName} is not found in DbContext configuration.");
            }

            IRelationalEntityTypeAnnotations mapping = rootEntityType.Relational();
            if(string.IsNullOrEmpty(mapping.Schema))
            {
                return $"[{mapping.TableName}]";
            }
            else
            {
                return $"[{mapping.Schema}].[{mapping.TableName}]";
            }
        }

        /// <summary>
        /// Get name of the column used by EF.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="context">DbContext</param>
        /// <param name="property">Property of entity that is used to get column name</param>
        /// <returns></returns>
        public static string GetColumnName<T>(this DbContext context, Expression<Func<T, object>> expression)
        {
            List<string> propertyNamePath = ReflectionUtility.GetMemberNamePath(expression);

            Type rootEntityType = typeof(T);
            string propertyName = ReflectionUtility.ConcatenateEfPropertyName(propertyNamePath);
            return GetColumnName(context, rootEntityType, propertyName);
        }

        /// <summary>
        /// Get name of the column used by EF.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">DbContext</param> 
        /// <param name="rootEntityType">Entity type to find property of. In case of owned entity specify parent entity type.</param>
        /// <param name="expression">Property of entity that is used to get column name.</param>
        /// <returns></returns>
        internal static string GetColumnName(this DbContext context, Type rootEntityType, MemberExpression expression)
        {
            List<string> propertyNamePath = ReflectionUtility.GetMemberPath(expression);
            string propertyName = ReflectionUtility.ConcatenateEfPropertyName(propertyNamePath);
            return GetColumnName(context, rootEntityType, propertyName);
        }

        /// <summary>
        /// Get name of the column used by EF.
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="rootEntityType">Entity type to find property of. In case of owned entity specify parent entity type.</param>
        /// <param name="propertyName">Property of entity. In case of owned entity specify propertyName of format of "NavigationProperty_OwnedEntityProperty".</param>
        /// <returns></returns>
        public static string GetColumnName(this DbContext context, Type rootEntityType, string propertyName)
        {
            IProperty property = GetPropertyMapping(context, rootEntityType, propertyName);
            IRelationalPropertyAnnotations propertyAnnotations = property.Relational();
            return propertyAnnotations.ColumnName;
        }

        /// <summary>
        /// Get list of properties that configured to be DatabaseGenerated with option DatabaseGeneratedOption.Identity or DatabaseGeneratedOption.Computed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<string> GetDatabaseGeneratedProperties<T>(this DbContext context)
            where T : class
        {
            Type typeName = typeof(T);
            return GetDatabaseGeneratedProperties(context, typeName);
        }

        /// <summary>
        /// Get list of properties that configured to be DatabaseGenerated with option DatabaseGeneratedOption.Identity or DatabaseGeneratedOption.Computed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="rootEntityType"></param>
        /// <returns></returns>
        public static List<string> GetDatabaseGeneratedProperties(this DbContext context, Type rootEntityType)
        {
            string entityName = TypeExtensions.DisplayName(rootEntityType);
            IEntityType rootEntity = context.Model.FindEntityType(entityName);

            List<string> keyNames = rootEntity.GetProperties()
                .Where(x => x.ValueGenerated == ValueGenerated.OnAdd
                    || x.ValueGenerated == ValueGenerated.OnAddOrUpdate)
                .Select(property =>
                {
                    IRelationalPropertyAnnotations propertyAnnotations = property.Relational();
                    return propertyAnnotations.ColumnName;
                })
                .ToList();

            return keyNames;
        }

        /// <summary>
        ///  Get IProperty configuration of the column used by EF.
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="rootEntityType">Entity type to find property of. In case of owned entity specify parent entity type.</param>
        /// <param name="propertyName">Property of entity. In case of owned entity specify propertyName of format of "NavigationProperty_OwnedEntityProperty".</param>
        /// <returns></returns>
        public static IProperty GetPropertyMapping(this DbContext context, Type rootEntityType, string propertyName)
        {
            string entityName = TypeExtensions.DisplayName(rootEntityType);
            IEntityType rootEntity = context.Model.FindEntityType(entityName);

            if (rootEntity == null)
            {
                throw new KeyNotFoundException($"Entity {rootEntityType.FullName} is not found in EntityFramework configuration.");
            }

            List<string> propertyNamePath = ReflectionUtility.SplitEfPropertyName(propertyName);
            IProperty property = null;

            if (propertyNamePath.Count == 1)
            {
                property = rootEntity.FindProperty(propertyName);
            }
            else if (propertyNamePath.Count == 2)
            {
                string rootEntityNavigationProperty = propertyNamePath[0];
                string ownedEntityProperty = propertyNamePath[1];

                IEntityType ownedEntity = GetOwnedProperty(context, rootEntity, rootEntityNavigationProperty);
                property = ownedEntity.FindProperty(ownedEntityProperty);
            }
            else
            {
                throw new NotImplementedException("More than 1 level of Entity framework owned entities is not supported.");
            }

            if (property == null)
            {
                
                throw new KeyNotFoundException($"Property {propertyName} is not found in EntityFramework configuration of {rootEntityType.FullName} type.");
            }

            return property;
        }

        public static IEntityType GetOwnedProperty(this DbContext context, 
            IEntityType rootEntity, string navigationProperty)
        {
            INavigation navigationProp = rootEntity
                .FindNavigation(navigationProperty);

            IEntityType ownedEntity = context.Model.GetEntityTypes()
                .FirstOrDefault(
                    x => x.ClrType == navigationProp.ClrType
                    && x.IsOwned());

            return ownedEntity;
        }


        /// <summary>
        /// Get list of all mapped properties for a given entity.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<string> GetAllMappedProperties<T>(this DbContext context)
        {
            Type typeName = typeof(T);
            return GetAllMappedProperties(context, typeName);
        }

        /// <summary>
        /// Get list of all mapped properties for a given entity.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="rootEntityType"></param>
        /// <returns></returns>
        public static List<string> GetAllMappedProperties(this DbContext context, Type rootEntityType)
        {
            string entityName = TypeExtensions.DisplayName(rootEntityType);
            IEntityType rootEntity = context.Model.FindEntityType(entityName);
            IEnumerable<IProperty> entityProperties = rootEntity.GetProperties();

            List<string> keyNames = entityProperties
                .Select(x => x.Name)
                .ToList();

            return keyNames;
        }

    }
}
