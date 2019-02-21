using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentComparator.Internal
{
    public static class Extensions
    {
        public static Func<object, object> ToGeneric<T, TProperty>(this Func<T, TProperty> func) => x => func((T)x);
        public static Func<object, int> ToGeneric<T>(this Func<T, int> func) => x => func((T)x);
        public static Func<object, long> ToGeneric<T>(this Func<T, long> func) => x => func((T)x);
        public static Func<object, decimal> ToGeneric<T>(this Func<T, decimal> func) => x => func((T)x);
        public static Func<object, double> ToGeneric<T>(this Func<T, double> func) => x => func((T)x);
        public static Func<object, bool> ToGeneric<T>(this Func<T, bool> func) => x => func((T)x);
        public static Func<object, string> ToGeneric<T>(this Func<T, string> func) => x => func((T)x);
        public static Func<object, Task<string>> ToGeneric<T>(this Func<T, Task<string>> func) => x => func((T)x);
        public static Func<object, object, bool> ToGeneric<T1, T2>(this Func<T1, T2, bool> func) => (x, y) => func((T1)x, (T2)y);

        public static void Add(this ExpandoObject _, string key, object value) => ((IDictionary<string, object>)_).Add(key, value);

        public static MemberInfo GetMember(this LambdaExpression expression)
        {
            var memberExp = RemoveUnary(expression.Body) as MemberExpression;
            if (memberExp == null) return null;
            return memberExp.Member;
        }

        public static MemberInfo GetMember<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            var memberExp = RemoveUnary(expression.Body) as MemberExpression;
            if (memberExp == null) return null;
            var currentExpr = memberExp.Expression;
            while (true)
            {
                currentExpr = RemoveUnary(currentExpr);
                if (currentExpr != null && currentExpr.NodeType == ExpressionType.MemberAccess)
                    currentExpr = ((MemberExpression)currentExpr).Expression;
                else
                    break;
            }

            if(currentExpr == null || currentExpr.NodeType != ExpressionType.Parameter) return null;

            return memberExp.Member;
        }
        private static Expression RemoveUnary(Expression toUnwrap)
        {
            if (toUnwrap is UnaryExpression) return ((UnaryExpression)toUnwrap).Operand;
            return toUnwrap;
        }

    }
}
