using TS.MediatR;
using System.Diagnostics;

namespace Cost.Accounting.Automation.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        Debug.WriteLine($"[LOG] Handling {requestName} - {DateTime.Now}");

        var response = await next();

        Debug.WriteLine($"[LOG] Handled {requestName} - {DateTime.Now}");

        return response;
    }
}