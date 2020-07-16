using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Abstractions.Events
{
    public interface IEventFactory
    {
        void SetEvent(Type service, Type implement);

        Type GetEvent(Type service);

        Type GetOrAddEvent(Type serviceType, Type implementType);

        List<T> GetOrAddEvent<T>(Type serviceType, Type implementType);

        List<TImplement> GetOrAddEvent<TService, TImplement>();
    }

    public class EventFactory : IEventFactory
    {
        private readonly ConcurrentDictionary<Type, Type> _types;
        private readonly IServiceProvider _serviceProvider;

        public EventFactory(IServiceProvider serviceProvider)
        {
            _types = new ConcurrentDictionary<Type, Type>();
            _serviceProvider = serviceProvider;
        }

        public Type GetEvent(Type service)
        {
            if (_types.TryGetValue(service, out Type implement))
            {
                return implement;
            }
            return null;
        }

        public List<T> GetOrAddEvent<T>(Type serviceType, Type implementType)
        {
            var type = GetOrAddEvent(serviceType, implementType);

            using (var scope = _serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider.GetServices(type);

                var service = services.FirstOrDefault();
                if (service == null)
                    return null;

                if (!(service is T))
                    return null;

                return services.Select(x => (T)x).ToList();
            }
        }

        public Type GetOrAddEvent(Type serviceType, Type implementType)
        {
            return _types.GetOrAdd(serviceType, (service) => implementType);
        }

        public List<TImplement> GetOrAddEvent<TService, TImplement>()
        {
            var type = GetOrAddEvent(typeof(TService), typeof(TImplement));

            using (var scope = _serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider.GetServices(type);

                var service = services.FirstOrDefault();
                if (service == null)
                    return null;

                if (!(service is TImplement))
                    return null;

                return services.Select(x => (TImplement)x).ToList();
            }
        }

        public void SetEvent(Type service, Type implement)
        {
            _types.TryAdd(service, implement);
        }
    }
}