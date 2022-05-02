namespace ConfiguratorDotNet.Generator;

internal class MapSchemaElement : SchemaElement
{
    private readonly Dictionary<string, SchemaElement> children = new();

    public MapSchemaElement(SchemaElement? parent) : base(parent)
    {
    }

    public override IEnumerable<SchemaElement> Children => this.children.Values;

    public void AddChild(string tagName, SchemaElement child)
    {
        this.children.Add(tagName, child);
    }
}
