﻿namespace ConfiguratorDotNet.Schema;

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
                $"Failed to parse '{element.Value}' as a type of '{this.ScalarType.TypeName}'.",
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
        this.XmlElement.Value = element.Value;
    }

    public override int GetHashCode()
    {
        return this.ScalarType.TypeName.GetHashCode();
    }
}