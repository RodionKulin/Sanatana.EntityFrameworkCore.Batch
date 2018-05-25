﻿using Sanatana.EntityFrameworkCore.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.ColumnMapping
{
    public class MappingComponent<TEntity> : MappingComponentBase<TEntity>
        where TEntity : class
    {

        //init
        public MappingComponent(List<MappedProperty> allEntityProperties
               , MappedPropertyUtility mappedPropertyUtility)
            : base(allEntityProperties, mappedPropertyUtility)
        {
        }


        //methods
        public virtual MappingComponent<TEntity> IncludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _includePropertyEfDefaultNames.Add(propName);
            return this;
        }

        public virtual MappingComponent<TEntity> ExcludeProperty<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            string propName = ReflectionUtility.GetDefaultEfMemberName(property);
            _excludePropertyEfDefaultNames.Add(propName);
            return this;
        }
    }
}
