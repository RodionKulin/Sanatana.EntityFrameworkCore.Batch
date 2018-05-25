using Sanatana.EntityFrameworkCore.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Sanatana.EntityFrameworkCore
{
    public static class EnumerableExtensions
    {
        /// <summary> 
        /// Create DataTable from entity list. 
        /// </summary> 
        public static DataTable ToDataTable<T>(this IEnumerable<T> source, List<string> propertyOrder = null)
        {
            DataTable table = new DataTable();

            BindingFlags binding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;
            PropertyReflectionOptions options = PropertyReflectionOptions.IgnoreEnumerable |
                PropertyReflectionOptions.IgnoreIndexer;

            Type stringType = typeof(string);
            List<PropertyInfo> properties = ReflectionUtility.GetProperties<T>(binding, options)
                .Where(p => p.PropertyType == stringType || !p.PropertyType.IsClass)
                .ToList();

            if (propertyOrder != null)
            {
                properties = properties.Where(p => propertyOrder.Contains(p.Name)).ToList();
                properties = propertyOrder.Join(properties,
                             k => k,
                             m => m.Name,
                             (k, i) => i)
                             .ToList();
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type nestedNullableType = property.PropertyType.GetGenericArguments()[0];
                    table.Columns.Add(property.Name, nestedNullableType);
                }
                else if (property.PropertyType.IsEnum)
                {
                    table.Columns.Add(property.Name, typeof(int));
                }
                else
                {
                    table.Columns.Add(property.Name, property.PropertyType);
                }
            }


            foreach (T item in source)
            {
                List<object> rowValues = new List<object>();

                for (int i = 0; i < properties.Count; i++)
                {
                    object propValue = properties[i].GetValue(item, null);

                    if (propValue == null)
                        rowValues.Add(DBNull.Value);
                    else if (properties[i].PropertyType.IsEnum)
                        rowValues.Add((int)propValue);
                    else
                        rowValues.Add(propValue);
                }

                table.Rows.Add(rowValues.ToArray());
            }

            return table;
        }
    }
}
