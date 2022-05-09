namespace Mixable.Schema;

/// <summary>
/// Represents a scalar element.
/// </summary>
public class ScalarSchemaElement : SchemaElement
{
    public ScalarSchemaElement(
        ScalarType scalarType,
        XElement element)
        : base(element)
    {
        this.ScalarType = scalarType;
    }

    public ScalarType ScalarType { get; }

    public override T Accept<T>(ISchemaVisitor<T> visitor)
    {
        return visitor.Accept(this);
    }

    protected internal override bool MatchesSchema(
        XElement element,
        MatchKind matchKind,
        IAttributeValidator validator,
        IErrorCollector errorCollector)
    {
        bool returnValue = true;

        validator.Validate(element, errorCollector);

        List<XElement> children = element.GetChildren().ToList();
        if (children.Count > 0)
        {
            errorCollector.Error(
                "Override schemas may not introduce children to scalar nodes.",
                this.XmlElement.GetDocumentPath());

            returnValue = false;
        }

        if (!this.ScalarType.Parser.CanParse(element.Value))
        {
            errorCollector.Error(
                $"Failed to parse '{element.Value}' as a type of '{this.ScalarType.Type}'.",
                this.XmlElement.GetDocumentPath());

            returnValue = false;
        }

        return returnValue;
    }

    protected internal override void MergeWithProtected(
        XElement element,
        IAttributeValidator validator,
        IErrorCollector collector)
    {
        base.MergeWithProtected(element, validator, collector);
        this.XmlElement.Value = element.Value;
    }
}
