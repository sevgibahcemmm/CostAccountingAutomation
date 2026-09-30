using FluentValidation;
using FluentValidation.Results;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Behaviors;
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var errorDictionary = _validators
            .Select(s => s.Validate(context))
            .SelectMany(s => s.Errors)
            .Where(s => s != null)
            .GroupBy(
            s => s.PropertyName,
            s => s.ErrorMessage, (propertyName, errorMessage) => new
            {
                Key = propertyName,
                Values = errorMessage.Distinct().ToArray()
            })
            .ToDictionary(s => s.Key, s => s.Values[0]);

        if (errorDictionary.Any())
        {
            // Key = özellik adı, Value = hata metni. ValidationFailure'da bu ikisi
            // ters sırada atanırsa ErrorMessage boş kalır ve çağıran taraf
            // kullanıcıya gösterilecek mesajı göremez.
            var errors = errorDictionary.Select(s => new ValidationFailure
            {
                PropertyName = s.Key,
                ErrorMessage = s.Value
            });
            throw new ValidationException(errors);
        }

        return await next();
    }
}