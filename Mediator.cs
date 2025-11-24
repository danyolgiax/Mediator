using Doxo.Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Doxo.Mediator
{
    public class Mediator:IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);

            var pipelineTypes = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var behaviors = _serviceProvider.GetServices(pipelineTypes).Cast<dynamic>().ToList();

            RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.Handle((dynamic)request, cancellationToken);

            foreach (var behavior in behaviors.AsEnumerable().Reverse())
            {
                var next = handlerDelegate;
                handlerDelegate = () => behavior.Handle((dynamic)request, cancellationToken, next);
            }

            return await handlerDelegate();
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
            foreach (var handler in handlers)
            {
                await handler.Handle(notification, cancellationToken);
            }
        }
    }
}