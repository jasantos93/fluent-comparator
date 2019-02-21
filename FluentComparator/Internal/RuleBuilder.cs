using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FluentComparator.Internal
{
    public class RuleBuilder<T, TProperty>
    {
        internal PropertyRule Rule { get; set; }
        internal AbstractComparator<T> Configuration { get; set; }
        internal RuleBuilder(PropertyRule rule, AbstractComparator<T> configuration)
        {
            Rule = rule;
            Configuration = configuration;
        }

        private Expression<Func<T, T, bool>> EqualExpression()
        {
            var a = Expression.Parameter(typeof(T), "a");
            var b = Expression.Parameter(typeof(T), "b");

            var property = Rule.Expression;

            var propA = Expression.Invoke(property, a);
            var propB = Expression.Invoke(property, b);

            var operation = Expression.Equal(Expression.Convert(propA, Rule.PropetyType), Expression.Convert(propB, Rule.PropetyType));
            var lambda = Expression.Lambda<Func<T, T, bool>>(operation, a, b);
            return lambda;
        }

        private Expression<Func<T, TX, bool>> EqualToExpression<TX>(TX value)
        {
            var a = Expression.Parameter(typeof(T), "a");
            var b = Expression.Parameter(typeof(TX), "b");
            var property = Rule.Expression;

            var propA = Expression.Invoke(property, a);
            var propB = Expression.Constant(value);

            var operation = Expression.Equal(Expression.Convert(propA, Rule.PropetyType), Expression.Convert(propB, Rule.PropetyType));
            var lambda = Expression.Lambda<Func<T, TX, bool>>(operation, a, b);
            return lambda;
        }


        public RuleBuilder<T, TProperty> Compare()
        {
            var expression = EqualExpression();
            Compare(expression);
            return this;
        }

        public RuleBuilder<T, TProperty> CompareTo<TX>(TX value)
        {
            var expression = EqualToExpression(value);
            Rule.Data.Compare = expression.Compile().ToGeneric();
            return this;
        }

        public RuleBuilder<T, TProperty> Compare(Expression<Func<T, T, bool>> func)
        {
            Rule.Data.Compare = func.Compile().ToGeneric();
            return this;
        }

        public RuleBuilder<T, TProperty> WithMessage(string message)
        {
            Rule.Data.GetMessage = (a) => message;
            return this;
        }

        public RuleBuilder<T, TProperty> WithoutMessage()
        {
            Rule.Data.WithoutMessage = true;
            return this;
        }
        public RuleBuilder<T, TProperty> WithMessage(Func<T, string> func)
        {
            Rule.Data.GetMessage = func.ToGeneric();
            return this;
        }
        public RuleBuilder<T, TProperty> WithMessageAsync(Func<T, Task<string>> func)
        {
            Rule.Data.GetMessageAsync = func.ToGeneric();
            return this;
        }
        public RuleBuilder<T, TProperty> Ignore()
        {
            Rule.Data.IsIgnored = true;
            return this;
        }
    }
}
