namespace ConfiguratorDotNet.Generator;

internal class ScalarSchemaElement : SchemaElement
{
    public ScalarSchemaElement(string typeName, string? customParser, SchemaElement? parent)
        : base(parent)
    {
        this.TypeName = typeName;
        this.CustomParser = customParser;
    }

    public string? CustomParser { get; set; }
}
