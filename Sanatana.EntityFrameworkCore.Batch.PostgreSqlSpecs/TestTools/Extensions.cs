using StructureMap.AutoMocking;
using System;
using System.Collections.Generic;
using System.Text;
using SpecsFor.Core;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools
{
    public static class Extensions
    {
        public static AutoMockedContainer GetContainer(this IAutoMocker autoMocker)
        {
            AutoMockedContainer autoMockContainer = (autoMocker as dynamic).MoqAutoMocker.Container;
            return autoMockContainer;
        }

        public static T GetServiceInstance<T>(this IAutoMocker autoMocker)
        {
            AutoMockedContainer autoMockContainer = (autoMocker as dynamic).MoqAutoMocker.Container;
            return autoMockContainer.GetInstance<T>();
        }
    }
}
