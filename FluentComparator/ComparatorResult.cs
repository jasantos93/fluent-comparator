using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace FluentComparator
{

    public class ComparatorExistsResult<T>
    {
        public bool Exists { get; internal set; }
        public T Data { get; internal set; }
    }
    
    public class ComparatorResult : Dictionary<MemberInfo, string>
    {
        public ComparatorResult()
        {
        }
        public ComparatorResult(IDictionary<MemberInfo, string> dictionary) : base(dictionary) { }
        public ExpandoObject Data { get; internal set; }

        public bool IsDifferent { get; internal set; }
    }
}
