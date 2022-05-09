namespace Mixable.Schema;

/// <summary>
/// Represents a list of elements with a shared schema.
/// </summary>
public class ListSchemaElement : SchemaElement
{
    public ListSchemaElement(
        XElement node,
        SchemaElement templateChild) : base(node)
    {
        this.Template = templateChild;
    }

    public SchemaElement Template { get; }

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

        foreach (XElement child in element.GetFilteredChildren())
        {
            if (child.Name != this.Template.XmlElement.Name)
            {
                errorCollector.Error($"Expected tag name: '{this.Template.XmlElement.Name}'. Got: '{child.Name}'.", child.GetDocumentPath());
                returnValue = false;
            }

            // Always use strict matching for lists. A subset of an item isn't good enough
            // here.
            returnValue &= this.Template.MatchesSchema(
                child,
                MatchKind.Strict,
                validator,
                errorCollector);
        }

        return returnValue;
    }

    protected internal override void MergeWithProtected(
        XElement element,
        IAttributeValidator validator,
        IErrorCollector collector)
    {
        base.MergeWithProtected(element, validator, collector);
        MetadataAttributes attrs = validator.Validate(element, collector);

        if (attrs.ListMergePolicy == ListMergePolicy.Replace || this.Modifier == NodeModifier.Abstract)
        {
            this.XmlElement.RemoveNodes();
        }

        if (this.Modifier != NodeModifier.Abstract)
        {
            foreach (var child in element.GetFilteredChildren())
            {
                this.XmlElement.Add(child);
            }
        }
    }
}
