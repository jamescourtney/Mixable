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

    public override bool MatchesSchema(
        XElement element,
        IAttributeValidator validator,
        out string path,
        out string error)
    {
        if (!validator.TryValidate(element, out path, out error, out _))
        {
            return false;
        }

        foreach (XElement child in element.GetFilteredChildren())
        {
            if (child.Name != this.Template.XmlElement.Name)
            {
                path = child.GetDocumentPath();
                error = $"Expected tag name: '{this.Template.XmlElement.Name}'. Got: '{child.Name}'.";
                return false;
            }

            if (!this.Template.MatchesSchema(child, validator, out path, out error))
            {
                return false;
            }
        }

        path = string.Empty;
        error = string.Empty;
        return true;
    }

    public override void MergeWith(XElement element, IAttributeValidator validator)
    {
        MetadataAttributes attrs = validator.Validate(element);

        if (!element.GetFilteredChildren().Any())
        {
            // no elements.
            if (attrs.ListMergePolicy == ListMergePolicy.Replace)
            {
                this.XmlElement.RemoveNodes();
            }

            return;
        }

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
