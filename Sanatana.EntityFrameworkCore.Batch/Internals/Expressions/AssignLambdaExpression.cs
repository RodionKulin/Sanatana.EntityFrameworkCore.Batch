﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Internals.Expressions
{
    public class AssignLambdaExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }


    }
}
