﻿using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Providers
{
    public class TransactionScopeWrapper : Behavior<INeedSampleDatabase>
    {
        //fields
        private TransactionScope _scope;


        //methods
        public override void SpecInit(INeedSampleDatabase instance)
        {
            _scope = new TransactionScope(TransactionScopeOption.RequiresNew);
        }

        public override void Given(INeedSampleDatabase instance)
        {
            base.Given(instance);
        }

        public override void ClassUnderTestInitialized(INeedSampleDatabase instance)
        {
            base.ClassUnderTestInitialized(instance);
        }

        public override void AfterSpec(INeedSampleDatabase instance)
        {
            if (_scope != null)
            {
                _scope.Dispose();
            }
        }
    }
}
