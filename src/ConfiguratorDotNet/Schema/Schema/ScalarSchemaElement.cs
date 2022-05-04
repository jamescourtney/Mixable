namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Represents a scalar element.
/// </summary>
public class ScalarSchemaElement : SchemaElement
{
    public ScalarSchemaElement(
        ScalarType scalarType,
        SchemaElement? parent,
        XElement element)
        : base(parent, element)
    {
        this.ScalarType = scalarType;
    }

    public ScalarType ScalarType { get; }

    public override T Accept<T>(ISchemaVisitor<T> visitor)
    {
        return visitor.Accept(this);
    }

    public override bool Equals(SchemaElement? other)
    {
        if (other is not ScalarSchemaElement scalar)
        {
            return false;
        }

        return scalar.ScalarType.TypeName == this.ScalarType.TypeName;
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
            mismatchPath = this.XmlElement.GetDocumentPath();
            error = "Override schemas may not introduce children to scalar nodes.";
            return false;
        }

        if (!this.ScalarType.Parser.CanParse(element.Value))
        {
            mismatchPath = this.XmlElement.GetDocumentPath();
            error = $"Failed to parse '{element.Value}' as a type of '{this.ScalarType.TypeName}'.";
            return false;
        }

        mismatchPath = string.Empty;
        error = string.Empty;
        return true;
    }

    public override void MergeWith(XElement element, IAttributeValidator validator)
    {
        this.XmlElement.Value = element.Value;
    }

    public override int GetHashCode()
    {
        return this.ScalarType.TypeName.GetHashCode();
    }
}
