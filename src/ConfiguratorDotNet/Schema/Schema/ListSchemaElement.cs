namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Represents a list of elements with a shared schema.
/// </summary>
public class ListSchemaElement : SchemaElement
{
    public ListSchemaElement(
        SchemaElement? parent,
        XElement node,
        SchemaElement templateChild) : base(parent, node)
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
        IAttributeValidator validator,
        IErrorCollector errorCollector)
    {
        bool returnValue = true;
        validator.Validate(element, errorCollector);

        foreach (XElement child in element.GetFilteredChildren())
        {
            if (child.Attribute(Constants.Structure.ListTemplateAttributeName) is not null)
            {
                continue;
            }

            if (child.Name != this.Template.XmlElement.Name)
            {
                errorCollector.Error($"Expected tag name: '{this.Template.XmlElement.Name}'. Got: '{child.Name}'.", child.GetDocumentPath());
                returnValue = false;
            }

            returnValue &= this.Template.MatchesSchema(child, validator, errorCollector);
        }

        return returnValue;
    }

    protected internal override void MergeWithProtected(
        XElement element,
        IAttributeValidator validator,
        IErrorCollector collector)
    {
        MetadataAttributes attrs = validator.Validate(element, collector);

        if (attrs.ListMergePolicy == ListMergePolicy.Replace)
        {
            this.XmlElement.RemoveNodes();
        }
        
        foreach (var child in element.GetFilteredChildren())
        {
            this.XmlElement.Add(child);
        }
    }

    public override bool Equals(SchemaElement? other)
    {
        if (other is not ListSchemaElement list)
        {
            return false;
        }

        bool thisHasChildren = this.Template is not null;
        bool otherHasChildren = list.Template is not null;

        if (thisHasChildren != otherHasChildren)
        {
            return false;
        }

        if (!thisHasChildren)
        {
            return true;
        }

        return this.Template == list.Template;
    }

    public override int GetHashCode()
    {
        return this.Template.GetHashCode() ^ "List".GetHashCode();
    }
}
