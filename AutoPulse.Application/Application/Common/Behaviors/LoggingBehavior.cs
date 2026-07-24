using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Application.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for MediatR to correlate traces and log execution details.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ActivitySource ActivitySource = new("AutoPulse.Application");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        using var activity = ActivitySource.StartActivity($"MediatR.{requestName}");

        if (activity is not null)
        {
            activity.SetTag("mediatr.request_name", requestName);
        }

        logger.LogInformation("Iniciando procesamiento de la solicitud {RequestName}", requestName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            logger.LogInformation("Finalizado procesamiento de la solicitud {RequestName} en {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);

            activity?.SetTag("mediatr.status", "Ok");
            activity?.SetTag("mediatr.duration_ms", stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("mediatr.status", "Error");
            activity?.SetTag("mediatr.duration_ms", stopwatch.ElapsedMilliseconds);

            var traceId = Activity.Current?.TraceId.ToString() ?? "N/A";
            logger.LogError(ex, "Error procesando la solicitud {RequestName} en {ElapsedMilliseconds}ms. [TraceId: {TraceId}]", 
                requestName, stopwatch.ElapsedMilliseconds, traceId);
            throw;
        }
    }
}
