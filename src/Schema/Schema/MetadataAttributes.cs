namespace ConfiguratorDotNet.Schema;

public record struct MetadataAttributes
{
    public string? TypeName { get; init; }

    public ListMergePolicy? ListMergePolicy { get; init; }

    public bool? List { get; init; }

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
            TypeName = element.Attribute(Constants.Attributes.Type)?.Value,

            List = element.Attribute(Constants.Attributes.List)?.Value?.ToLowerInvariant() switch
            {
                "true" => true,
                "false" => false,
                _ => null,
            },

            ListMergePolicy = listMerge,

            Optional = element.Attribute(Constants.Attributes.Optional)?.Value?.ToLowerInvariant() == "true",
        };
    }
}
