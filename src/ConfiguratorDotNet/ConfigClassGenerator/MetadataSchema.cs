namespace ConfiguratorDotNet.Generator;

internal class MetadataOnlyDocument
{
    [YamlDotNet.Serialization.YamlMember(Alias = Constants.MetadataPropertyName)]
    public MetadataNode? Metadata { get; set; }
}

internal class MetadataNode
{
    /// <summary>
    /// The base schema, as a relative path from the current file.
    /// </summary>
    public string? BaseSchemaFile { get; set; }

    /// <summary>
    /// The output file name, as a relative path from the current file.
    /// </summary>
    public string? OutputFileName { get; set; }
}