namespace ConfiguratorDotNet.Generator;

internal class ScalarSchemaElement : SchemaElement
{
    public ScalarSchemaElement(string typeName, string? customParser, SchemaElement? parent, XElement element)
        : base(parent, element)
    {
        this.TypeName = typeName;
        this.CustomParser = customParser;
    }

    public string? CustomParser { get; set; }

    public override bool Equals(SchemaElement? other)
    {
        if (other is not ScalarSchemaElement scalar)
        {
            return false;
        }

        return scalar.TypeName == this.TypeName
            && scalar.CustomParser == this.CustomParser;
    }

    public override void Validate()
    {
    }

    public override int GetHashCode()
    {
        return this.TypeName.GetHashCode() ^ this.CustomParser?.GetHashCode() ?? 0;
    }
}
