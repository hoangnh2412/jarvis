using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Infrastructure
{
    public interface IModuleManager
    {
        Type GetImplementation<T>();
        Type GetImplementation<T>(Func<ModuleInfo, bool> predicate);
        IEnumerable<Type> GetImplementations<T>();
        IEnumerable<Type> GetImplementations<T>(Func<ModuleInfo, bool> predicate);
        Type GetImplementation(Type type);
        Type GetImplementation(Type type, Func<ModuleInfo, bool> predicate);
        IEnumerable<Type> GetImplementations(Type type);
        IEnumerable<Type> GetImplementations(Type type, Func<ModuleInfo, bool> predicate);
        T GetInstance<T>();
        T GetInstance<T>(params object[] args);
        T GetInstance<T>(Func<ModuleInfo, bool> predicate);
        T GetInstance<T>(Func<ModuleInfo, bool> predicate, params object[] args);
        IEnumerable<T> GetInstances<T>();
        IEnumerable<T> GetInstances<T>(params object[] args);
        IEnumerable<T> GetInstances<T>(Func<ModuleInfo, bool> predicate);
        IEnumerable<T> GetInstances<T>(Func<ModuleInfo, bool> predicate, params object[] args);
        Type GetInterface<T>();
        Type GetInterface<T>(Func<ModuleInfo, bool> predicate);
        IEnumerable<Type> GetInterfaces<T>();
        IEnumerable<Type> GetInterfaces<T>(Func<ModuleInfo, bool> predicate);
        Type GetInterface(Type type);
        Type GetInterface(Type type, Func<ModuleInfo, bool> predicate);
        IEnumerable<Type> GetInterfaces(Type type);
        IEnumerable<Type> GetInterfaces(Type type, Func<ModuleInfo, bool> predicate);
    }

    /// <summary>
    /// Represents the assembly cache with the mechanism of getting implementations or instances of a given type.
    /// This is the global access point to the ExtCore type discovering mechanism.
    /// </summary>
    public class ModuleManager : IModuleManager
    {
        private ConcurrentDictionary<string, Assembly> _assemblies;
        private ConcurrentDictionary<Type, IEnumerable<Type>> _implementations;
        private ConcurrentDictionary<Type, IEnumerable<Type>> _interfaces;

        public ModuleManager()
        {
            _assemblies = new ConcurrentDictionary<string, Assembly>();
            _implementations = new ConcurrentDictionary<Type, IEnumerable<Type>>();
            _interfaces = new ConcurrentDictionary<Type, IEnumerable<Type>>();
        }

        /// <summary>
        /// Gets the first implementation of the type specified by the type parameter, or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>The first found implementation of the given type.</returns>
        public Type GetImplementation<T>()
        {
            return GetImplementation<T>(null);
        }

        /// <summary>
        /// Gets the first implementation of the type specified by the type parameter and located in the assemblies
        /// filtered by the predicate, or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>The first found implementation of the given type.</returns>
        public Type GetImplementation<T>(Func<ModuleInfo, bool> predicate)
        {
            return GetImplementations<T>(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the implementations of the type specified by the type parameter.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>Found implementations of the given type.</returns>
        public IEnumerable<Type> GetImplementations<T>()
        {
            return GetImplementations<T>(null);
        }

        /// <summary>
        /// Gets the implementations of the type specified by the type parameter and located in the assemblies
        /// filtered by the predicate.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>Found implementations of the given type.</returns>
        public IEnumerable<Type> GetImplementations<T>(Func<ModuleInfo, bool> predicate)
        {
            return GetImplementations(typeof(T), predicate);
        }

        /// <summary>
        /// Gets the first implementation of the type specified by the type parameter, or null if no implementations found.
        /// </summary>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>The first found implementation of the given type.</returns>
        public Type GetImplementation(Type type)
        {
            return GetImplementation(null);
        }

        /// <summary>
        /// Gets the first implementation of the type specified by the type parameter and located in the assemblies
        /// filtered by the predicate, or null if no implementations found.
        /// </summary>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>The first found implementation of the given type.</returns>
        public Type GetImplementation(Type type, Func<ModuleInfo, bool> predicate)
        {
            return GetImplementations(type, predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the implementations of the type specified by the type parameter.
        /// </summary>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>Found implementations of the given type.</returns>
        public IEnumerable<Type> GetImplementations(Type type)
        {
            return GetImplementations(type, null);
        }

        /// <summary>
        /// Gets the implementations of the type specified by the type parameter and located in the assemblies
        /// filtered by the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>Found implementations of the given type.</returns>
        public IEnumerable<Type> GetImplementations(Type type, Func<ModuleInfo, bool> predicate)
        {
            if (_implementations.ContainsKey(type))
                return _implementations[type];

            // var ass1 = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            // var ass3 = Assembly.GetCallingAssembly().GetReferencedAssemblies();

            // var type1 = Assembly.GetExecutingAssembly().GetTypes();
            // var type2 = Assembly.GetExecutingAssembly().GetExportedTypes();
            // var type3 = Assembly.GetExecutingAssembly().GetForwardedTypes(); ;

            // var type4 = Assembly.GetEntryAssembly().GetTypes();
            // var type5 = Assembly.GetEntryAssembly().GetExportedTypes();
            // var type6 = Assembly.GetEntryAssembly().GetForwardedTypes();

            // var type7 = Assembly.GetCallingAssembly().GetTypes();
            // var type8 = Assembly.GetCallingAssembly().GetExportedTypes();
            // var type9 = Assembly.GetCallingAssembly().GetForwardedTypes();

            // var types = Assembly.GetEntryAssembly().GetExportedTypes();

            List<Type> implementations = new List<Type>();
            foreach (var assembly in _assemblies)
            {
                var types = assembly.Value.GetExportedTypes();
                implementations.AddRange(GetImplementations(types, type));
            }

            if (implementations.Count == 0)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => IsCandidateCompilationLibrary(x.FullName) && !_assemblies.Keys.Contains(x.GetName().FullName))
                    .ToList();
                foreach (var assembly in assemblies)
                {
                    _assemblies.TryAdd(assembly.GetName().FullName, assembly);
                    var types = assembly.GetExportedTypes();
                    implementations.AddRange(GetImplementations(types, type));
                }
            }

            _implementations.TryAdd(type, implementations);

            return implementations;
        }

        /// <summary>
        /// Gets the new instance of the first implementation of the type specified by the type parameter,
        /// or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the instance(s) of the same type(s) is requested.
        /// </param>
        /// <returns>The instance of the first found implementation of the given type.</returns>
        public T GetInstance<T>()
        {
            return GetInstance<T>(null, new object[] { });
        }

        /// <summary>
        /// Gets the new instance (using constructor that matches the arguments) of the first implementation
        /// of the type specified by the type parameter or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the instance(s) of the same type(s) is requested.
        /// </param>
        /// <param name="args">The arguments to be passed to the constructor.</param>
        /// <returns>The instance of the first found implementation of the given type.</returns>
        public T GetInstance<T>(params object[] args)
        {
            return GetInstance<T>(null, args);
        }

        /// <summary>
        /// Gets the new instance of the first implementation of the type specified by the type parameter
        /// and located in the assemblies filtered by the predicate or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the instance(s) of the same type(s) is requested.
        /// </param>
        /// <returns>The instance of the first found implementation of the given type.</returns>
        public T GetInstance<T>(Func<ModuleInfo, bool> predicate)
        {
            return GetInstances<T>(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the new instance (using constructor that matches the arguments) of the first implementation
        /// of the type specified by the type parameter and located in the assemblies filtered by the predicate
        /// or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the instance(s) of the same type(s) is requested.
        /// </param>
        /// <param name="args">The arguments to be passed to the constructor.</param>
        /// <returns>The instance of the first found implementation of the given type.</returns>
        public T GetInstance<T>(Func<ModuleInfo, bool> predicate, params object[] args)
        {
            return GetInstances<T>(predicate, args).FirstOrDefault();
        }

        /// <summary>
        /// Gets the new instances of the implementations of the type specified by the type parameter
        /// or empty enumeration if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the instance(s) of the same type(s) is requested.
        /// </param>
        /// <returns>The instances of the found implementations of the given type.</returns>
        public IEnumerable<T> GetInstances<T>()
        {
            return GetInstances<T>(null, new object[] { });
        }

        /// <summary>
        /// Gets the new instances (using constructor that matches the arguments) of the implementations
        /// of the type specified by the type parameter or empty enumeration if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the instance(s) of the same type(s) is requested.
        /// </param>
        /// <param name="args">The arguments to be passed to the constructors.</param>
        /// <returns>The instances of the found implementations of the given type.</returns>
        public IEnumerable<T> GetInstances<T>(params object[] args)
        {
            return GetInstances<T>(null, args);
        }

        /// <summary>
        /// Gets the new instances of the implementations of the type specified by the type parameter
        /// and located in the assemblies filtered by the predicate or empty enumeration
        /// if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the instance(s) of the same type(s) is requested.
        /// </param>
        /// <returns>The instances of the found implementations of the given type.</returns>
        public IEnumerable<T> GetInstances<T>(Func<ModuleInfo, bool> predicate)
        {
            return GetInstances<T>(predicate, new object[] { });
        }

        /// <summary>
        /// Gets the new instances (using constructor that matches the arguments) of the implementations
        /// of the type specified by the type parameter and located in the assemblies filtered by the predicate
        /// or empty enumeration if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the instance(s) of the same type(s) is requested.
        /// </param>
        /// <param name="args">The arguments to be passed to the constructors.</param>
        /// <returns>The instances of the found implementations of the given type.</returns>
        public IEnumerable<T> GetInstances<T>(Func<ModuleInfo, bool> predicate, params object[] args)
        {
            List<T> instances = new List<T>();

            foreach (Type implementation in GetImplementations<T>(predicate))
            {
                if (!implementation.GetTypeInfo().IsAbstract)
                {
                    T instance = (T)Activator.CreateInstance(implementation, args);

                    instances.Add(instance);
                }
            }

            return instances;
        }


        /// <summary>
        /// Gets the first implementation of the type specified by the type parameter, or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>The first found implementation of the given type.</returns>
        public Type GetInterface<T>()
        {
            return GetInterface<T>(null);
        }

        /// <summary>
        /// Gets the first implementation of the type specified by the type parameter and located in the assemblies
        /// filtered by the predicate, or null if no implementations found.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementation of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>The first found implementation of the given type.</returns>
        public Type GetInterface<T>(Func<ModuleInfo, bool> predicate)
        {
            return GetInterfaces<T>(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the implementations of the type specified by the type parameter.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>Found implementations of the given type.</returns>
        public IEnumerable<Type> GetInterfaces<T>()
        {
            return GetInterfaces<T>(null);
        }

        /// <summary>
        /// Gets the implementations of the type specified by the type parameter and located in the assemblies
        /// filtered by the predicate.
        /// </summary>
        /// <typeparam name="T">The type parameter to find implementations of.</typeparam>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>Found implementations of the given type.</returns>
        public IEnumerable<Type> GetInterfaces<T>(Func<ModuleInfo, bool> predicate)
        {
            return GetInterfaces(typeof(T), predicate);
        }

        /// <summary>
        /// Gets the first implementation of the type specified by the type parameter, or null if no implementations found.
        /// </summary>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>The first found implementation of the given type.</returns>
        public Type GetInterface(Type type)
        {
            return GetInterface(null);
        }

        /// <summary>
        /// Gets the first implementation of the type specified by the type parameter and located in the assemblies
        /// filtered by the predicate, or null if no implementations found.
        /// </summary>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>The first found implementation of the given type.</returns>
        public Type GetInterface(Type type, Func<ModuleInfo, bool> predicate)
        {
            return GetInterfaces(type, predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the implementations of the type specified by the type parameter.
        /// </summary>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>Found implementations of the given type.</returns>
        public IEnumerable<Type> GetInterfaces(Type type)
        {
            return GetInterfaces(type, null);
        }

        /// <summary>
        /// Gets the implementations of the type specified by the type parameter and located in the assemblies
        /// filtered by the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter the assemblies.</param>
        /// Determines whether the type cache should be used to avoid assemblies scanning next time,
        /// when the same type(s) is requested.
        /// </param>
        /// <returns>Found implementations of the given type.</returns>
        public IEnumerable<Type> GetInterfaces(Type type, Func<ModuleInfo, bool> predicate)
        {
            if (_interfaces.ContainsKey(type))
                return _interfaces[type];

            List<Type> interfaces = new List<Type>();
            foreach (var assembly in _assemblies)
            {
                var types = assembly.Value.GetExportedTypes();
                interfaces.AddRange(GetInterfaces(types, type));
            }

            if (interfaces.Count == 0)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => IsCandidateCompilationLibrary(x.FullName) && !_assemblies.Keys.Contains(x.GetName().FullName))
                    .ToList();
                foreach (var assembly in assemblies)
                {
                    _assemblies.TryAdd(assembly.GetName().FullName, assembly);
                    var types = assembly.GetExportedTypes();
                    interfaces.AddRange(GetInterfaces(types, type));
                }
            }

            _interfaces.TryAdd(type, interfaces);

            return interfaces;
        }

        private static List<Type> GetInterfaces(Type[] types, Type type)
        {
            List<Type> implementations = new List<Type>();
            foreach (Type item in types)
            {
                var typeInfo = type.GetTypeInfo();
                var exportedTypeInfo = item.GetTypeInfo();
                if (exportedTypeInfo.IsInterface)
                {
                    if (type != item && typeInfo.IsAssignableFrom(item))
                    {
                        implementations.Add(item);
                    }
                    else //generic type
                    {
                        var genericInterface = item.GetInterface(type.Name);
                        if (genericInterface != null)
                        {
                            implementations.Add(item);
                        }
                    }
                }
            }

            return implementations;
        }

        private bool IsCandidateCompilationLibrary(string name)
        {
            return !name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("Newtonsoft.", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("runtime.", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("NLog", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("Swashbuckle", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("Remotion", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("Anonymously Hosted DynamicMethods Assembly", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("NETStandard.Library", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("Libuv", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("Remotion.Linq", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("StackExchange.Redis.StrongName", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("WindowsAzure.Storage", StringComparison.OrdinalIgnoreCase);
        }

        private static List<Type> GetImplementations(Type[] types, Type type)
        {
            List<Type> implementations = new List<Type>();
            foreach (Type item in types)
            {
                var typeInfo = type.GetTypeInfo();
                var exportedTypeInfo = item.GetTypeInfo();
                if (exportedTypeInfo.IsClass)
                {
                    if (typeInfo.IsAssignableFrom(item))
                    {
                        if (!implementations.Contains(item))
                        {
                            implementations.Add(item);
                        }
                    }
                    else //generic type
                    {
                        var genericInterface = item.GetInterface(type.FullName);
                        if (genericInterface != null)
                        {
                            if (!implementations.Contains(item))
                            {
                                implementations.Add(item);
                            }
                        }
                    }
                }
            }

            return implementations;
        }
    }
}
