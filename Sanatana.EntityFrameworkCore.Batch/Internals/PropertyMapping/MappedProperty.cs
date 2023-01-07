using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping
{
    public class MappedProperty : IEquatable<MappedProperty>
    {
        //properties
        public List<MappedProperty> ChildProperties { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        /// <summary>
        /// Name of the property of POCO. Can be concatenated of multiple property names for nested objects.
        /// </summary>
        public string PocoPropertyName { get; set; }
        /// <summary>
        /// Name of the column in database.
        /// </summary>
        public string DbColumnName { get; set; }
        public string ConfiguredSqlType { get; set; }
        public object Value { get; set; }



        //dependent properties
        public bool IsComplexProperty
        {
            get
            {
                return ChildProperties != null;
            }
        }



        //methods
        public virtual MappedProperty Clone()
        {
            return new MappedProperty()
            {
                PropertyInfo = PropertyInfo,
                PocoPropertyName = PocoPropertyName,
                DbColumnName = DbColumnName,
                ConfiguredSqlType = ConfiguredSqlType
            };
        }


        //compare
        public override int GetHashCode()
        {
            return PropertyInfo.GetHashCode();
        }

        public bool Equals(MappedProperty other)
        {
            return this.PropertyInfo == other.PropertyInfo;
        }
    }
}
