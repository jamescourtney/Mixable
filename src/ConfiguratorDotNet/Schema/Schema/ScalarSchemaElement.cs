namespace ConfiguratorDotNet.Schema;

internal class ScalarSchemaElement : SchemaElement
{
    private readonly ScalarType scalarType;

    public ScalarSchemaElement(
        ScalarType scalarType,
        SchemaElement? parent,
        XElement element)
        : base(parent, element)
    {
        this.TypeName = scalarType.TypeName;
        this.scalarType = scalarType;
    }

    public string? CustomParser { get; set; }

    public override bool Equals(SchemaElement? other)
    {
        if (other is not ScalarSchemaElement scalar)
        {
            return false;
        }

        return scalar.TypeName == this.TypeName;
    }

    public override bool MatchesSchema(
        XElement element,
        IAttributeValidator validator,
        out string mismatchPath,
        out string error)
    {
        if (!validator.TryValidate(element, out mismatchPath, out error, out _))
        {
            return false;
        }

        List<XElement> children = element.GetChildren().ToList();
        if (children.Count > 0)
        {
            mismatchPath = this.XPath;
            error = "Override schemas may not introduce children to scalar nodes.";
            return false;
        }

        if (!this.scalarType.Parser.CanParse(element.Value))
        {
            mismatchPath = this.xElement.GetDocumentPath();
            error = $"Failed to parse '{element.Value}' as a type of '{this.TypeName}'.";
            return false;
        }

        mismatchPath = string.Empty;
        error = string.Empty;
        return true;
    }

    public override void MergeWith(XElement element, IAttributeValidator validator)
    {
        this.xElement.Value = element.Value;
    }

    public override int GetHashCode()
    {
        return this.TypeName.GetHashCode() ^ this.CustomParser?.GetHashCode() ?? 0;
    }
}
