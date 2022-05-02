namespace ConfiguratorDotNet.Generator;

internal class ListSchemaElement : SchemaElement
{
    private readonly List<SchemaElement> children = new();

    public ListSchemaElement(SchemaElement? parent) : base(parent)
    {
    }

    public override IEnumerable<SchemaElement> Children => this.children;

    public void AddChild(SchemaElement element)
    {
        this.TypeName = $"List<{element.TypeName}>";
        this.children.Add(element);
    }
}
