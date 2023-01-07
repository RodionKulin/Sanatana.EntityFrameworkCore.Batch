using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Internals.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Internals.Expressions
{
    public static class ExpressionsToSql
    {
        //constants
        public static readonly string[] DEFAULT_ALIASES = new[] {
            "Extent1", "Extent2", "Extent3", "Extent4",
            "Extent5", "Extent6", "Extent7", "Extent8",
            "Extent9", "Extent10", "Extent11", "Extent12",
            "Extent13", "Extent14", "Extent15", "Extent16"
        };


        public static bool? TryGetBooleanBody(Expression expression, IDbParametersService dbParametersService)
        {
            string value = expression.ToSqlString(dbParametersService);
            if(value == dbParametersService.FormatBoolean(true))
            {
                return true;
            }
            else if(value == dbParametersService.FormatBoolean(false))
            {
                return false;
            }

            return null;
        }

        //linq to sql string
        public static string ToSqlString(this Expression expression, IDbParametersService dbParametersService, DbContext? context = null, bool useLambdaAlias = true,
            string?[] alternativeAliases = null)
        {
            ExpressionContext details = new ExpressionContext(expression, context, useLambdaAlias);
            details.AlternativeAliases = alternativeAliases;
            details.DbParametersService = dbParametersService;
            return ToSqlString(expression, details);
        }

        private static string ToSqlString(this Expression expression, ExpressionContext details)
        {
            //assign
            if (expression is AssignLambdaExpression)
            {
                var assign = expression as AssignLambdaExpression;
                string assignLeft = assign.Left.ToSqlString(details.Copy());
                string assignRight = assign.Right.ToSqlString(details.Copy());
                return assignLeft + " = " + assignRight;
            }

            switch (expression.NodeType)
            {
                //lambda
                case ExpressionType.Lambda:
                    var lambda = expression as LambdaExpression;
                    details.SetArguments(lambda);
                    return lambda.Body.ToSqlString(details);

                //call
                case ExpressionType.Call:
                    var method = expression as MethodCallExpression;
                    return ContainsToSql(method, details);

                //value
                case ExpressionType.Constant:
                    var constant = expression as ConstantExpression;
                    return ConstantToSql(constant.Value, constant.Type, details);

                case ExpressionType.MemberAccess:
                    var memberAccess = expression as MemberExpression;
                    return MemberToSql(memberAccess, details);

                case ExpressionType.Convert:
                    UnaryExpression unary = expression as UnaryExpression;
                    return ConvertToSql(unary, details);

                //compare
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThanOrEqual:
                    var binaryCompareExpression = expression as BinaryExpression;
                    return BinaryCompareToSql(binaryCompareExpression, details);

                //math and logical
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    var binarySetExpression = expression as BinaryExpression;
                    return BinarySetToSql(binarySetExpression, details);
            }

            throw new NotImplementedException(
                $"Unknown type of {nameof(ExpressionType)} {expression.GetType().ToString()} and NodeType {expression.NodeType.ToString()}");
        }


        //methods
        private static string ContainsToSql(MethodCallExpression expression, ExpressionContext details)
        {
            //method type
            bool isContains = expression.Method.Name == "Contains";
            if (isContains == false)
                throw new NotImplementedException("Methods other than Contains are not supported in SQL expressions.");

            //argument
            Expression containsArgExp = expression.Arguments.FirstOrDefault();
            if (expression.Arguments.Count == 0)
                throw new NotImplementedException("Argument for Contains method is not provided.");

            Type valueType = containsArgExp.Type;
            string argSql = containsArgExp.ToSqlString(details);

            //values list
            var memberObject = expression.Object as MemberExpression;
            if (memberObject == null)
                throw new NotImplementedException("Unknown list type. Could not convert expression to MemberExpression.");

            IEnumerable objectList = MemberToList(memberObject);

            //join into expression IN
            StringBuilder joinListBuilder = new StringBuilder();
            foreach (var item in objectList)
            {
                string value = ConstantToSql(item, valueType, details);
                joinListBuilder.AppendFormat("{0}, ", value);
            }

            if (joinListBuilder.Length > 0)
            {
                joinListBuilder = joinListBuilder.Remove(joinListBuilder.Length - 2, 2);
            }

            return string.Format("{0} in ({1})", argSql, joinListBuilder.ToString());
        }


        //member
        private static string MemberToSql(MemberExpression expression, ExpressionContext details)
        {
            ParameterExpression parameterExpression = GetParameterExpression(expression);

            //expression with lambda left side (parameter)
            if (parameterExpression != null)
            {
                string alias = GetAlias(details, parameterExpression);
                string columnName = GetColumnName(expression, details);
                return string.IsNullOrEmpty(alias)
                    ? $"[{columnName}]"
                    : $"{alias}.[{columnName}]";
            }
            //object property value
            else if (expression.Member is PropertyInfo)
            {
                PropertyInfo property = (PropertyInfo)expression.Member;
                bool isStatic = property.GetGetMethod().IsStatic;

                if (isStatic)
                {
                    object value = property.GetValue(null);
                    return ConstantToSql(value, property.PropertyType, details);
                }
                else
                {
                    var member = (MemberExpression)expression.Expression;
                    var constant = (ConstantExpression)member.Expression;
                    var field = ((FieldInfo)member.Member).GetValue(constant.Value);
                    var value = ((PropertyInfo)expression.Member).GetValue(field);
                    return ConstantToSql(value, property.PropertyType, details);
                }
            }
            //constant value
            else if (expression.Member is FieldInfo)
            {
                var field = (FieldInfo)expression.Member;
                bool isStatic = field.IsStatic;

                if (isStatic)
                {
                    object value = field.GetValue(null);
                    return ConstantToSql(value, field.FieldType, details);
                }
                else
                {
                    var constant = (ConstantExpression)expression.Expression;
                    object value = field.GetValue(constant.Value);
                    return ConstantToSql(value, field.FieldType, details);
                }
            }
            else
            {
                throw new Exception($"Unknown type of expression {details.ParentExpression}");
            }
        }

        private static ParameterExpression GetParameterExpression(MemberExpression expression)
        {
            while (expression != null)
            {
                if (expression.Expression is ParameterExpression)
                {
                    ParameterExpression argumentExpression = expression.Expression as ParameterExpression;
                    return argumentExpression;
                }
                else if (expression.Expression is MemberExpression)
                {
                    expression = expression.Expression as MemberExpression;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        private static Type GetRootEntityType(MemberExpression expression)
        {
            while (expression != null)
            {
                if (expression.Expression is ParameterExpression)
                {
                    ParameterExpression parameter = expression.Expression as ParameterExpression;
                    Type entityType = parameter.Type;

                    return entityType;
                }
                else if (expression.Expression is MemberExpression)
                {
                    expression = expression.Expression as MemberExpression;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        private static IEnumerable MemberToList(MemberExpression expression)
        {
            Expression innerExpression = expression.Expression;

            bool hasValue = innerExpression.NodeType == ExpressionType.Constant;
            if (hasValue)
            {
                var constantExpression = innerExpression as ConstantExpression;
                FieldInfo field = ((FieldInfo)expression.Member);
                object memberValue = field.GetValue(constantExpression.Value);

                return (IEnumerable)memberValue;
            }
            else
            {
                throw new NotImplementedException("Unknown enumerable type.");
            }
        }

        private static string GetColumnName(MemberExpression expression, ExpressionContext details)
        {
            if (details.DbContext == null)
            {
                return ReflectionService.GetDefaultEfMemberName(expression);
            }
            else
            {
                Type dbSetEntityType = GetRootEntityType(expression);
                return details.DbContext.GetColumnName(dbSetEntityType, expression);
            }
        }

        private static string? GetAlias(ExpressionContext details, ParameterExpression argumentExpression)
        {
            if (details.Arguments == null)
            {
                throw new ArgumentNullException("Lambda arguments were not provided.");
            }

            if (!details.UseLambdaAlias)
            {
                return null;
            }

            int argIndex = details.Arguments.IndexOf(argumentExpression.Name);

            string[] aliases = details.AlternativeAliases ?? DEFAULT_ALIASES;
            if(aliases.Length <= argIndex)
            {
                throw new ArgumentException($"Provided {nameof(details.AlternativeAliases)} does not have enough aliases to use 0-based value {argIndex}.");
            }

            return aliases[argIndex];
        }


        //convert
        private static string ConvertToSql(UnaryExpression expression, ExpressionContext details)
        {
            UnaryExpression unary = expression as UnaryExpression;
            string value = unary.Operand.ToSqlString(details);
            return value;
        }


        //constant
        public static string ConstantToSql(object value, Type valueType, IDbParametersService _dbParametersService)
        {
            return ConstantToSql(value, valueType, new ExpressionContext(null, null, false)
            {
                DbParametersService = _dbParametersService
            });
        }

        public static string ConstantToSql(object value, Type valueType, ExpressionContext details)
        {
            if (Nullable.GetUnderlyingType(valueType) != null)
            {
                valueType = Nullable.GetUnderlyingType(valueType);
            }


            if (value == null)
            {
                return "NULL";
            }
            else if (valueType.IsEnum)
            {
                int intValue = (int)value;
                return intValue.ToString();
            }
            else if (valueType == typeof(bool))
            {
                return details.DbParametersService.FormatBoolean((bool)value);
            }
            else if (valueType == typeof(string))
            {
                return string.Format("N'{0}'", value.ToString().Replace("'", "''"));
            }
            else if (valueType == typeof(Guid))
            {
                return string.Format("cast('{0}' as uniqueidentifier)", value.ToString());
            }
            else if (valueType == typeof(DateTime))
            {
                DateTime time = (DateTime)value;
                return string.Format("cast('{0}-{1}-{2} {3}' as datetime2)",
                    time.Year, time.Month, time.Day, time.TimeOfDay.ToString());
            }

            //default
            return value.ToString();
        }


        //binary
        private static string BinaryCompareToSql(BinaryExpression expression, ExpressionContext details)
        {
            string operation;
            string leftPart = expression.Left.ToSqlString(details);
            string rightPart = expression.Right.ToSqlString(details);

            //change left and right sides if NULL is in the left
            if (leftPart == "NULL" && rightPart != "NULL")
            {
                leftPart = rightPart;
                rightPart = "NULL";
            }

            //IS NULL
            if (rightPart == "NULL")
            {
                if (expression.NodeType == ExpressionType.Equal)
                    operation = "IS";
                else if (expression.NodeType == ExpressionType.NotEqual)
                    operation = "IS NOT";
                else
                    throw new NotImplementedException($"Unknown type of {nameof(ExpressionType)} in expression " + expression.ToString() + ". Compare to null can only use symbols '=' or '!='.");
            }
            //operators = != > < >= <= 
            else
            {
                operation = ConvertOperationType(expression.NodeType);
            }

            return string.Format("{0} {1} {2}", leftPart, operation, rightPart);
        }

        private static string BinarySetToSql(BinaryExpression expression, ExpressionContext details)
        {
            string left = expression.Left.ToSqlString(details);
            string right = expression.Right.ToSqlString(details);
            string operation = ConvertOperationType(expression.NodeType);

            if (expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse)
            {
                left = $"({left})";
                right = $"({right})";
            }

            return string.Format("{0} {1} {2}", left, operation, right);
        }

        private static string ConvertOperationType(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "AND";
                default:
                    throw new Exception($"Unknown type of {nameof(ExpressionType)} {nodeType.ToString()}");
            }
        }
        
    }
}
