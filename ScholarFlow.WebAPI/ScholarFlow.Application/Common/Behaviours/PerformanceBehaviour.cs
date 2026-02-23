using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json; // Add this using directive

namespace ScholarFlow.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;

    public PerformanceBehaviour(ILogger<TRequest> logger)
    {
        _timer = new Stopwatch();
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500) // Log requests taking longer than 500ms
        {
            var requestName = typeof(TRequest).Name;
            // Serialize the request object to JSON for safer logging, or just log its name
            var requestData = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = false }); 

            _logger.LogWarning("ScholarFlow Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds). Request Data: {RequestData}",
                requestName, elapsedMilliseconds, requestData);
        }

        return response;
    }
}