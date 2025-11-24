using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doxo.Mediator.Interfaces
{
    public interface IPipelineBehavior<TRequest, TResponse>
    {
        Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next);
    }

    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
}