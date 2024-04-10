using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Shared.Extensions;

namespace Jarvis.Persistence;

/// <summary>
/// Storage application launch setting parameters
/// </summary>
public static partial class InstanceStorage
{
    public static class ConnectionStringResolver
    {
        private static Dictionary<string, Type> InstanceTypes = new Dictionary<string, Type>();

        public static string Get<TContext>()
        {
            var name = nameof(TContext);

            if (InstanceTypes.ContainsKey(name))
                return InstanceTypes[name].AssemblyQualifiedName;

            return null;
        }

        public static void Set<TContext, TResolver>()
            where TContext : IStorageContext
            where TResolver : IConnectionStringResolver
        {
            var name = nameof(TContext);
            if (InstanceTypes.ContainsKey(name))
                return;

            InstanceTypes[name] = typeof(TResolver);
        }
    }

    public static class StorageContext
    {
        public static Dictionary<string, Type> InstanceTypes = new Dictionary<string, Type>();

        public static void Add<T>(Type type) where T : IStorageContext
        {
            if (InstanceTypes.ContainsKey(type.AssemblyQualifiedName))
                throw new Exception($"Storage context {type.Name} has been exist");

            if (!type.IsInstanceOfType<IStorageContext>())
                throw new Exception($"Storage context {type.Name} does not implement interface {nameof(IStorageContext)}");

            InstanceTypes.Add(type.AssemblyQualifiedName, type);
        }
    }
}