using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Apointo.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
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
        var stopwatch = Stopwatch.StartNew();
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName} with payload {@Request}", requestName, request);

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} successfully in {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.Elapsed.TotalMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            _logger.LogError(
                exception,
                "Handling {RequestName} failed after {ElapsedMilliseconds} ms with payload {@Request}",
                requestName,
                stopwatch.Elapsed.TotalMilliseconds,
                request);

            throw;
        }
    }
}

