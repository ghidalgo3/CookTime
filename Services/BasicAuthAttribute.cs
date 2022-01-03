using Microsoft.AspNetCore.Mvc;

namespace babe_algorithms
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BasicAuthAttribute : TypeFilterAttribute
    {
        public BasicAuthAttribute(string realm = @"KookTime") : base(typeof(BasicAuthFilter))
        {
            Arguments = new object[] { realm };
        }
    }
}