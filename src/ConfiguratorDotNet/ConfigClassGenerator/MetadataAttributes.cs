namespace ConfiguratorDotNet.Generator;

internal record struct MetadataAttributes
{
    public string? TypeName { get; init; }

    public string? CustomParser { get; init; }

    public ListMergePolicy? ListMergePolicy { get; init; }

    public bool? List { get; init; }

    internal static MetadataAttributes Extract(XElement element)
    {
        return new MetadataAttributes
        {
            TypeName = element.Attribute(Constants.Structure.TypeAttributeName)?.Value,

            CustomParser = element.Attribute(Constants.Structure.CustomParserAttributeName)?.Value,

            List = element.Attribute(Constants.Structure.ListAttributeName)?.Value?.ToLowerInvariant() switch
            {
                "true" => true,
                "false" => false,
                _ => null,
            },

            ListMergePolicy = element.Attribute(Constants.Structure.ListMergeStrategyName)?.Value?.ToLowerInvariant() switch
            {
                string s => (ListMergePolicy)Enum.Parse(typeof(ListMergePolicy), s, ignoreCase: true),
                _ => null,
            }
        };
    }
}
