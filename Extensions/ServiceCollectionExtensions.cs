using Doxo.Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Doxo.Mediator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddTransient<IMediator,Mediator>();

            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };

            foreach (var assembly in assemblies)
            {
                // IRequestHandler<,>
                var requestHandlers = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .SelectMany(t => t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                        .Select(i => new { Interface = i, Implementation = t }));

                foreach (var h in requestHandlers)
                    services.AddTransient(h.Interface, h.Implementation);

                // INotificationHandler<>
                var notificationHandlers = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .SelectMany(t => t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                        .Select(i => new { Interface = i, Implementation = t }));

                foreach (var h in notificationHandlers)
                    services.AddTransient(h.Interface, h.Implementation);

                // IPipelineBehavior<,> - open generics
                var openGenericBehaviors = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .Where(t => t.IsGenericTypeDefinition)
                    .Where(t => t.GetInterfaces()
                        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
                    .ToList();

                foreach (var behaviorType in openGenericBehaviors)
                {
                    services.AddTransient(typeof(IPipelineBehavior<,>), behaviorType);
                }

                // IPipelineBehavior<,> - closed generics (optional, for specific implementations)
                var closedGenericBehaviors = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .Where(t => !t.IsGenericTypeDefinition)
                    .SelectMany(t => t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                        .Select(i => new { Interface = i, Implementation = t }));

                foreach (var b in closedGenericBehaviors)
                    services.AddTransient(b.Interface, b.Implementation);
            }

            return services;
        }
    }
}