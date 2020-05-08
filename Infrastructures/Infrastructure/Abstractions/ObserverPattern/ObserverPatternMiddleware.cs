using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure;
using System.Linq;
using Infrastructure.Abstractions.ObserverPattern;

namespace Infrastructure.Abstractions.ObserverPattern
{
    public class ObserverPatternMiddleware
    {
        private readonly RequestDelegate _next;

        public ObserverPatternMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IModuleManager moduleManager)
        {
            var subjects = moduleManager.GetImplementations<ISubject>()
                .SelectMany(x => x.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubject<>)))
                .GroupBy(x => x.FullName)
                .Select(x => x.FirstOrDefault())
                .ToList();

            var subcribers = moduleManager.GetImplementations<ISubcriber>()
                .SelectMany(x => x.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubcriber<>)))
                .GroupBy(x => x.FullName)
                .Select(x => x.FirstOrDefault())
                .ToList();

            var topics = moduleManager.GetImplementations<ITopic>().ToList();
            foreach (var topic in topics)
            {
                var subjectByTopics = subjects.Where(x => x.GetGenericArguments().Any(y => y.FullName == topic.FullName)).ToList();
                var subcriberByTopics = subcribers.Where(x => x.GetGenericArguments().Any(y => y.FullName == topic.FullName)).ToList();
                foreach (var subjectByTopic in subjectByTopics)
                {
                    var serviceSubjects = context.RequestServices.GetServices(subjectByTopic);
                    foreach (var serviceSubject in serviceSubjects)
                    {
                        foreach (var subcriberByTopic in subcriberByTopics)
                        {
                            var serviceSubcribers = context.RequestServices.GetServices(subcriberByTopic);
                            foreach (var serviceSubcriber in serviceSubcribers)
                            {
                                var method = serviceSubject.GetType().GetMethod("Subcribe");
                                method.Invoke(serviceSubject, new object[] { serviceSubcriber });
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}