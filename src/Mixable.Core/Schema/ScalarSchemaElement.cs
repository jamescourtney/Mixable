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

    public override T Accept<T>(ISchemaVisitor<T> visitor, IErrorCollector errorCollector)
    {
        return visitor.Accept(this, errorCollector);
    }

    protected internal override bool MatchesSchema(
        XElement element,
        MatchKind matchKind,
        IAttributeValidator validator,
        IErrorCollector errorCollector)
    {
        bool returnValue = true;

        var attrs = validator.Validate(element, errorCollector);

        List<XElement> children = element.GetChildren().ToList();
        if (children.Count > 0)
        {
            errorCollector.Error(
                "Override schemas may not introduce children to scalar nodes.",
                this.XmlElement.GetDocumentPath());

            returnValue = false;
        }

        if (attrs.Modifier is NodeModifier.Abstract or NodeModifier.Optional)
        {
            // Abstract/optional nodes not required to parse successfully.
        }
        else if (!this.ScalarType.Parser.CanParse(element.Value))
        {
            errorCollector.Error(
                $"Failed to parse '{element.Value}' as a type of '{this.ScalarType.Type}'.",
                this.XmlElement.GetDocumentPath());

            returnValue = false;
        }

        return returnValue;
    }

    protected override void MergeWithProtected(
        XElement element,
        bool allowAbstract,
        IAttributeValidator validator,
        IErrorCollector collector)
    {
        MetadataAttributes attributes = validator.Validate(element, collector);

        if (this.Modifier == NodeModifier.Final)
        {
            collector.Error(
                $"Nodes marked as '{NodeModifier.Final}' may not be overridden.",
                this.XmlElement);
        }

        if (this.Modifier != attributes.Modifier)
        {
            this.Modifier = attributes.Modifier;
            this.XmlElement.SetAttributeValue(Constants.Attributes.Flags, this.Modifier.ToString());
        }

        if (this.Modifier == NodeModifier.Abstract)
        {
            this.XmlElement.RemoveNodes();
        }
        else
        {
            this.XmlElement.Value = element.Value;
        }
    }
}
