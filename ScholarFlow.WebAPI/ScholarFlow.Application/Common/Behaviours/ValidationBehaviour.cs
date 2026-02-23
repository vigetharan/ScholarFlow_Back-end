using FluentValidation;
using MediatR;
using ScholarFlow.Application.Common.Models; // Ensure this is correct for your Result<T>
using System.Reflection; // Needed for Reflection

namespace ScholarFlow.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class // Constraint: TResponse must be a class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                // Check if TResponse is of type Result<TInner>
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    // Get the generic argument (TInner) of Result<TInner>
                    Type innerResultType = typeof(TResponse).GetGenericArguments()[0];

                    // Dynamically get the static Failure method from Result<TInner>
                    // We need to find the specific Failure(List<string> errors) overload
                    MethodInfo? failureMethod = typeof(Result<>)
                        .MakeGenericType(innerResultType)
                        .GetMethod(
                            nameof(Result<object>.Failure), // Use nameof for safety
                            BindingFlags.Public | BindingFlags.Static,
                            null, // Binder
                            new[] { typeof(List<string>) }, // Argument types to match
                            null // Modifiers
                        );

                    if (failureMethod != null)
                    {
                        // Invoke the static Failure method with the list of failures
                        var failureResult = failureMethod.Invoke(null, new object[] { failures });
                        return (TResponse)failureResult!;
                    }
                }
                
                // If TResponse is not a Result<T> or the Failure method couldn't be invoked,
                // throw a standard validation exception.
                throw new ValidationException(failures);
            }
        }
        return await next();
    }
}