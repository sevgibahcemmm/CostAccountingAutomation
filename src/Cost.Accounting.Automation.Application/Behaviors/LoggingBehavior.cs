using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowRequestThresholdMs = 500;
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("{RequestName} işleniyor.", requestName);

        try
        {
            var response = await next();

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "{RequestName} tamamlandı ancak beklenenden yavaş sürdü ({ElapsedMilliseconds} ms).",
                    requestName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "{RequestName} başarıyla tamamlandı ({ElapsedMilliseconds} ms).",
                    requestName, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "{RequestName} işlenirken hata oluştu ({ElapsedMilliseconds} ms).",
                requestName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}





