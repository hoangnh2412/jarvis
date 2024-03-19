using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Shared.Extensions;

namespace Jarvis.Persistence;

/// <summary>
/// Storage application launch setting parameters
/// </summary>
public static partial class InstanceStorage
{
    public static string ConnectionStringResolver = typeof(SingleTenantConnectionStringResolver).AssemblyQualifiedName;

    public static class StorageContext
    {
        public static Dictionary<string, Type> Items = new Dictionary<string, Type>();

        public static void Add<T>(Type type) where T : IStorageContext
        {
            if (Items.ContainsKey(type.AssemblyQualifiedName))
                throw new Exception($"Storage context {type.Name} has been exist");

            if (!type.IsInstanceOfType<IStorageContext>())
                throw new Exception($"Storage context {type.Name} does not implement interface {nameof(IStorageContext)}");

            Items.Add(type.AssemblyQualifiedName, type);
        }
    }
}