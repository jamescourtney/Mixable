namespace ConfiguratorDotNet.Generator;

internal abstract class SchemaElement
{
    protected SchemaElement(SchemaElement? parent)
    {
        this.Parent = parent;
    }

    /// <summary>
    /// Gets or sets the type name for the current schema element.
    /// </summary>
    public string? TypeName { get; set; }

    public SchemaElement? Parent { get; }

    public virtual IEnumerable<SchemaElement> Children => Array.Empty<SchemaElement>();
}
