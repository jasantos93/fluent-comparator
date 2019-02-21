using FluentComparator.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentComparator
{
    public interface IComparator<T>
    {
        /// <summary>
        /// Compare object A against object B.
        /// </summary>
        /// <param name="objA"></param>
        /// <param name="objB"></param>
        /// <returns></returns>
        ComparatorResult CompareWith(T objA, T objB);

        /// <summary>
        /// Compare an object with a specific value.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="objA"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        ComparatorResult CompareWith<TProperty>(T objA, Expression<Func<T, TProperty>> property, TProperty value, string message = null);

        /// <summary>
        /// Exists object A within collection of object B.
        /// </summary>
        /// <param name="objA"></param>
        /// <param name="objsB"></param>
        /// <returns></returns>
        ComparatorExistsResult<T> Exists(T objA, IEnumerable<T> objsB);
        /// <summary>
        /// Exists collection of object A within collection of object B.
        /// </summary>
        /// <param name="objsA"></param>
        /// <param name="objsB"></param>
        /// <returns></returns>
        IEnumerable<ComparatorExistsResult<T>> Exists(IEnumerable<T> objsA, IEnumerable<T> objsB);

    }
    public class AbstractComparator
    {
        public static IComparator<TX> Builder<TX>(Action<AbstractComparator<TX>> configuration = null)
        {
            var comparator = new RuntimeComparator<TX>();
            configuration?.Invoke(comparator);
            return comparator;
        }

        public static IComparator<TX> Builder<TX>(string nameObjA, string nameObjB)
        {
            var comparator = new RuntimeComparator<TX>();
            comparator.NameOfObject(nameObjA, nameObjB);
            return comparator;
        }
    }

    public abstract class AbstractComparator<T> : AbstractComparator, IComparator<T>
    {

        public AbstractComparator(bool all = false)
        {
            if (all) RuleAllProperties();
        }

        //Register all property belong to object and add comparar functionality.
        private void RuleAllProperties()
        {
            var properties = Type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var propType = property.PropertyType;

                if (propType.IsValueType || propType == typeof(DateTime) || propType == typeof(DateTimeOffset) || propType == typeof(string))
                {
                    var item = Expression.Parameter(Type, "item");
                    var propExpression = Expression.Property(item, property.Name);
                    var expression = Expression.Lambda<Func<T, object>>(Expression.Convert(propExpression, typeof(object)), item);
                    RuleBaseFor(expression).Compare();
                }
            }
        }

        /// <summary>
        /// Type of object to comparar.
        /// </summary>
        public Type Type { get; } = typeof(T);
        /// <summary>
        /// List of rules to apply of each property.
        /// </summary>
        internal IList<PropertyRule> Rules { get; } = new List<PropertyRule>();
        public RuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression) where TProperty : struct => RuleBaseFor(expression);
        public RuleBuilder<T, DateTimeOffset> RuleFor(Expression<Func<T, DateTimeOffset>> expression) => RuleBaseFor(expression);
        public RuleBuilder<T, DateTime> RuleFor(Expression<Func<T, DateTime>> expression) => RuleBaseFor(expression);
        public RuleBuilder<T, string> RuleFor(Expression<Func<T, string>> expression) => RuleBaseFor(expression);
        public void NameOfObject(string nameObjA, string nameObjB)
        {
            NameObjA = nameObjA;
            NameObjB = nameObjB;
        }
        private string NameObjA = "A";
        private string NameObjB = "B";

        internal RuleBuilder<T, TProperty> RuleBaseFor<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var rule = PropertyRule.Create(expression);

            if (Rules.Any(p => p.MemberInfo.Name == rule.MemberInfo.Name))
                return new RuleBuilder<T, TProperty>(Rules.FirstOrDefault(p => p.MemberInfo.Name == rule.MemberInfo.Name), this);

            Rules.Add(rule);
            return new RuleBuilder<T, TProperty>(rule, this);
        }

        /// <summary>
        /// Comparar object 1 against object 2.
        /// </summary>
        /// <param name="objA"></param>
        /// <param name="objB"></param>
        /// <returns></returns>
        public ComparatorResult CompareWith(T objA, T objB)
        {
            var messages = new List<string>();
            var result = new ComparatorResult();
            var comparatorResultData = new ExpandoObject();
            foreach (var rule in Rules)
            {

                if (!rule.Data.IsIgnored && !rule.Data.Compare(objA, objB))
                {
                    var propData = new ExpandoObject();
                    propData.Add(NameObjA, rule.PropertyFunc(objA));
                    propData.Add(NameObjB, rule.PropertyFunc(objB));

                    comparatorResultData.Add(rule.MemberInfo.Name, propData);

                    if (!result.IsDifferent) result.IsDifferent = true;
                    var message = !rule.Data.WithoutMessage ? rule.Data.GetMessageAsync(objA).Result : default(string);
                    result.Add(rule.MemberInfo, message);

                }
            }
            result.Data = comparatorResultData;
            return result;
        }

        public ComparatorResult CompareWith<TProperty>(T objA, Expression<Func<T, TProperty>> property, TProperty value, string message = null)
        {
            var messages = new List<string>();
            var result = new ComparatorResult();
            var comparatorResultData = new ExpandoObject();
            var propertyRule = PropertyRule.Create(property);
            new RuleBuilder<T, TProperty>(propertyRule, this).CompareTo(value);
            if (!propertyRule.Data.IsIgnored && !propertyRule.Data.Compare(objA, value))
            {
                var propData = new ExpandoObject();
                propData.Add(NameObjA, propertyRule.PropertyFunc(objA));
                propData.Add(NameObjB, value);

                comparatorResultData.Add(propertyRule.MemberInfo.Name, propData);

                if (!result.IsDifferent) result.IsDifferent = true;
                result.Add(propertyRule.MemberInfo, message);
            }
            result.Data = comparatorResultData;
            return result;
        }

        public ComparatorExistsResult<T> Exists(T objA, IEnumerable<T> objsB)
        {
            var result = new ComparatorExistsResult<T>();
            var resultData = new ExpandoObject();
            foreach (var objB in objsB)
            {
                result.Exists = Rules.All(p => !p.Data.IsIgnored && p.Data.Compare(objA, objB));
                if (result.Exists) break;
            }
            if (!result.Exists) result.Data = objA;
            return result;
        }

        public IEnumerable<ComparatorExistsResult<T>> Exists(IEnumerable<T> objsA, IEnumerable<T> objsB)
        {
            var results = new List<ComparatorExistsResult<T>>();
            foreach (var item in objsA)
            {
                var result = Exists(item, objsB);
                if (result.Exists) continue;
                results.Add(result);
            }
            return results;
        }
    }
}
