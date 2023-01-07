using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Internals.Expressions
{
    public class ExpressionContext
    {
        //properties
        public List<string> Arguments { get; set; }
        public Expression ParentExpression { get; set; }
        public DbContext? DbContext { get; set; }
        public bool UseLambdaAlias { get; set; }
        public string?[] AlternativeAliases { get; set; }
        public IDbParametersService DbParametersService { get; set; }


        //init
        public ExpressionContext(Expression parentExpression, DbContext? context, bool useLambdaAlias)
        {
            ParentExpression = parentExpression;
            DbContext = context;
            UseLambdaAlias = useLambdaAlias;
        }


        //methods
        public void SetArguments(LambdaExpression lambda)
        {
            if (Arguments != null)
            {
                throw new Exception("Lambda arguments already set.");
            }

            Arguments = lambda.Parameters
                .Select(p => p.Name)
                .ToList();
        }

        public ExpressionContext Copy()
        {
            return new ExpressionContext(ParentExpression, DbContext, UseLambdaAlias)
            {
                Arguments = Arguments,
                AlternativeAliases = AlternativeAliases,
                DbParametersService = DbParametersService,
            };
        }

    }
}
