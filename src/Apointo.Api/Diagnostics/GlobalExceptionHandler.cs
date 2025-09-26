using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apointo.Api.Diagnostics;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception captured by global handler for {RequestPath} with trace {TraceId}",
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        var problemDetails = new ProblemDetails
        {
            Title = "Beklenmeyen bir hata oluştu.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = _hostEnvironment.IsDevelopment() ? exception.Message : "Lütfen daha sonra tekrar deneyin.",
            Instance = httpContext.Request.Path,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}


