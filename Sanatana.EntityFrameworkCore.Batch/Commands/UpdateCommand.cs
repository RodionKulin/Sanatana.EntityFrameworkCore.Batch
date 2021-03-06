﻿using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Commands
{
    public class UpdateCommand<TEntity>
        where TEntity : class
    {
        //fields
        protected DbContext _context;
        protected Expression<Func<TEntity, bool>> _matchExpression;
        protected List<Expression> _updateExpressions;


        //properties
        public long? Limit { get; set; }


        //init
        public UpdateCommand(DbContext context, Expression<Func<TEntity, bool>> matchExpression)
        {
            _context = context;
            _matchExpression = matchExpression;
            _updateExpressions = new List<Expression>();
        }


        //methods
        /// <summary>
        /// Expression to update columns of Target table. Example: (t) => t.IntProperty, (t) => t.OtherIntProperty * 2.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="targetProperty"></param>
        /// <param name="assignedValue"></param>
        /// <returns></returns>
        public virtual UpdateCommand<TEntity> Assign<TProp>(
            Expression<Func<TEntity, TProp>> targetProperty,
            Expression<Func<TEntity, TProp>> assignedValue)
        {
            _updateExpressions.Add(new AssignLambdaExpression()
            {
                Left = targetProperty,
                Right = assignedValue
            });
            return this;
        }

        /// <summary>
        /// Limit number or rows to be updated
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual UpdateCommand<TEntity> SetLimit(long? limit)
        {
            Limit = limit;
            return this;
        }

        public virtual int Execute()
        {
            string command = ContructUpdateManyCommand();
            int changes = _context.Database.ExecuteSqlRaw(command);

            return changes;
        }

        public virtual async Task<int> ExecuteAsync()
        {
            string command = ContructUpdateManyCommand();
            int changes = await _context.Database.ExecuteSqlRawAsync(command)
                .ConfigureAwait(false);

            return changes;
        }

        protected virtual string ContructUpdateManyCommand()
        {
            string tableName = _context.GetTableName<TEntity>();
            string matchSql = _matchExpression.ToMSSqlString(_context);

            List<string> assignParts = new List<string>();
            foreach (Expression item in _updateExpressions)
            {
                string expressionSql = item.ToMSSqlString(_context);
                assignParts.Add(expressionSql);
            }
            string updateSql = string.Join(", ", assignParts);
            string tableAlias = ExpressionsToMSSql.ALIASES[0];

            string limit = Limit == null
                ? string.Empty
                : $"TOP({Limit.Value}) ";
            string command = string.Format("UPDATE {0}{1} SET {2} FROM {3} AS {1} WHERE {4}",
                limit, tableAlias, updateSql, tableName, matchSql);
          
            return command;
        }
    }
}
