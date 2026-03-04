using FluentValidation;
using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var errorMessages = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            .ToList();

        if (!errorMessages.Any())
            return await next();

        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            var apiResponseType = typeof(ApiResponse<>).MakeGenericType(innerType);

            var failMethod = apiResponseType.GetMethod(
                nameof(ApiResponse<object>.Fail),
                new[] { typeof(string), typeof(List<string>) }
            );

            var firstError = errorMessages.First();
            var result = failMethod!.Invoke(null, new object?[] { firstError, errorMessages });
            return (TResponse)result!;
        }

        throw new ValidationException(
            errorMessages.Select(m => new FluentValidation.Results.ValidationFailure("", m))
        );
    }
}