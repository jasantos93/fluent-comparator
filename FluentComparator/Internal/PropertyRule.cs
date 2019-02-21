using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentComparator.Internal
{
    internal class PropertyData
    {
        private Func<object, Task<string>> _getMessageAsync;

        private const string DefaultMessage = "Difference found";
        public Func<object, string> GetMessage { get; set; } = (obj) => DefaultMessage;
        public Func<object, Task<string>> GetMessageAsync
        {
            get
            {
                if (GetMessage != null) return (obj) => Task.FromResult(GetMessage.Invoke(obj));
                return _getMessageAsync;
            }
            set => _getMessageAsync = value;
        }
        public bool WithoutMessage { get; set; }
        public bool IsIgnored { get; set; }
        public Func<object, object, bool> Compare { get; set; }

    }
    internal class PropertyRule
    {
        public Func<object, object> PropertyFunc { get; }
        public LambdaExpression Expression { get; }
        public Type PropetyType { get; }
        public PropertyData Data { get; }
        public MemberInfo MemberInfo { get; }
        public PropertyRule(Func<object, object> propertyFunc, LambdaExpression expression, Type type, MemberInfo memberInfo)
        {
            Data = new PropertyData();
            PropertyFunc = propertyFunc;
            Expression = expression;
            PropetyType = type;
            MemberInfo = memberInfo;
        }

        public static PropertyRule Create<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var compiated = expression.Compile();
            var member = expression.GetMember();
            var propType = ((PropertyInfo)member).PropertyType;
            return new PropertyRule(compiated.ToGeneric(), expression, propType, expression.GetMember());
        }

        public static PropertyRule Create<T>(Expression<Func<object, object>> expression)
        {
            var compiated = expression.Compile();
            var member = expression.GetMember();
            var propType = ((PropertyInfo)member).PropertyType;
            return new PropertyRule(compiated, expression, propType, expression.GetMember());
        }
    }
}
