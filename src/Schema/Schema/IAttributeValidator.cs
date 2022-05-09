namespace Mixable.Schema;

/// <summary>
/// Validates metadata attributes.
/// </summary>
public interface IAttributeValidator
{
    /// <summary>
    /// Gets the root attribute validator (as they are often chained).
    /// </summary>
    IAttributeValidator RootValidator { get; }

    /// <summary>
    /// Validates the given element's metadata attributes, returning the actual metadata.
    /// </summary>
    MetadataAttributes Validate(
        XElement element,
        IErrorCollector errorCollector);
}

public static class IAttributeValidatorExtensions
{
    public static IAttributeValidator DecorateWith(this IAttributeValidator validator, Func<MetadataAttributes, IErrorCollector, bool> callback)
    {
        if (validator is Decorator decorator)
        {
            return decorator.Extend(callback);
        }
        else
        {
            return new Decorator(validator, callback);
        }
    }

    private class Decorator : IAttributeValidator
    {
        private readonly IAttributeValidator validator;
        private readonly List<Func<MetadataAttributes, IErrorCollector, bool>> callbacks;

        public Decorator(IAttributeValidator validator, Func<MetadataAttributes, IErrorCollector, bool> callback) : this(validator)
        {
            this.callbacks.Add(callback);
        }

        private Decorator(IAttributeValidator validator)
        {
            this.callbacks = new();
            this.RootValidator = validator.RootValidator;
            this.validator = validator;
        }

        public IAttributeValidator Extend(Func<MetadataAttributes, IErrorCollector, bool> callback)
        {
            var d = new Decorator(this.RootValidator);
            d.callbacks.AddRange(this.callbacks);
            d.callbacks.Add(callback);

            return d;
        }

        public IAttributeValidator RootValidator { get; }

        public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
        {
            var attributes = this.validator.Validate(element, errorCollector);

            foreach (var callback in this.callbacks)
            {
                if (!callback(attributes, errorCollector))
                {
                    break;
                }
            }

            return attributes;
        }
    }
}