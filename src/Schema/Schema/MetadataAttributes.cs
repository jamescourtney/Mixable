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

public record struct MetadataAttributes
{
    public string? RawTypeName { get; init; }

    public WellKnownType? WellKnownType
    {
        get
        {
            if (Enum.TryParse<WellKnownType>(this.RawTypeName, ignoreCase: true, out var result))
            {
                return result;
            }

            return null;
        }
    }

    public ListMergePolicy? ListMergePolicy { get; init; }

    public bool Optional { get; init; }

    internal static MetadataAttributes Extract(XElement element, IErrorCollector? errorCollector)
    {
        ListMergePolicy? listMerge = null;
        if (element.Attribute(Constants.Attributes.ListMerge)?.Value is string value)
        {
            if (Enum.TryParse<ListMergePolicy>(value, ignoreCase: true, out var listMergeValue))
            {
                listMerge = listMergeValue;
            }
            else
            {
                errorCollector?.Error(
                    $"Unable to parse '{value}' as a list merge value. Valid values are: {string.Join(",", Enum.GetNames(typeof(ListMergePolicy)))}.",
                    element.GetDocumentPath());
            }
        }

        return new MetadataAttributes
        {
            RawTypeName = element.Attribute(Constants.Attributes.Type)?.Value,
            ListMergePolicy = listMerge,
            Optional = element.Attribute(Constants.Attributes.Optional)?.Value?.ToLowerInvariant() == "true",
        };
    }
}
