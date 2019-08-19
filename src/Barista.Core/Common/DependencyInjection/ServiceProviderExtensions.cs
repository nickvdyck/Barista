using System.Collections.Generic;
using System.Linq;

namespace Barista.Common.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static T GetInstance<T>(this ServiceProvider provider) =>
            (T)provider(typeof(T));

        public static IEnumerable<T> GetInstances<T>(this ServiceProvider provider) =>
            ((IEnumerable<object>)provider(typeof(IEnumerable<T>)))
                .Cast<T>();
    }
}
