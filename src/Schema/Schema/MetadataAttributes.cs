namespace Mixable.Schema;

public enum WellKnownType
{
    List = 0,
    Int = 1,
    String = 2,
    Double = 3,
    Bool = 4,
    Map = 5,
}

public enum NodeModifier
{
    None = 0,

    /// <summary>
    /// The node is optional. Valid only inside template declarations.
    /// </summary>
    Optional = 1,

    /// <summary>
    /// The node is abstract (must be overridden). Valid only in fully-rooted declarations.
    /// </summary>
    Abstract = 2,

    /// <summary>
    /// The node is final (cannot be overridden). Valid only in fully-rooted declarations.
    /// </summary>
    Final = 3,
}

public record struct MetadataAttributes
{
    public string? RawTypeName { get; init; }

    public WellKnownType? WellKnownType { get; private set; }

    public ListMergePolicy? ListMergePolicy { get; init; }

    public NodeModifier Modifier { get; init; }

    public XElement SourceElement { get; init; }

    internal static MetadataAttributes Extract(XElement element, IErrorCollector? errorCollector)
    {
        string? rawType = element.Attribute(Constants.Attributes.Type)?.Value;
        return new MetadataAttributes
        {
            SourceElement = element,
            RawTypeName = rawType,

            // Pass in null error collector since we don't need to report parse failures.
            WellKnownType = ParseEnum<WellKnownType>(rawType, element, errorCollector: null),

            Modifier = ParseEnum<NodeModifier>(element.Attribute(Constants.Attributes.Flags)?.Value, element, errorCollector) ?? NodeModifier.None,
            ListMergePolicy = ParseEnum<ListMergePolicy>(element.Attribute(Constants.Attributes.ListMerge)?.Value, element, errorCollector),
        };
    }

    private static T? ParseEnum<T>(
        string? value,
        XElement node,
        IErrorCollector? errorCollector) where T : struct, Enum
    {
        if (value is not null)
        {
            if (Enum.TryParse(value, ignoreCase: true, out T result))
            {
                return result;
            }
            else
            {
                errorCollector?.Error(
                    $"Unable to parse '{value}' as a '{typeof(T).Name}' value. Valid values are: {string.Join(",", Enum.GetNames(typeof(T)))}.",
                    node.GetDocumentPath());
            }
        }

        return null;
    }
}
