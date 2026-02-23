using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json; // Add this using directive

namespace ScholarFlow.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            // Serialize the request object to JSON for safer logging
            var requestData = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = false });

            _logger.LogError(ex, "ScholarFlow Request: Unhandled Exception for Request {Name}. Request Data: {RequestData}", requestName, requestData);

            throw;
        }
    }
}