namespace ConfiguratorDotNet.Generator;

internal class ListSchemaElement : SchemaElement
{
    private SchemaElement template;

    public ListSchemaElement(
        SchemaElement? parent,
        XElement node,
        SchemaElement templateChild) : base(parent, node)
    {
        this.template = templateChild;
        this.TypeName = $"List<{templateChild.TypeName}>";
    }

    public override IEnumerable<SchemaElement> Children => new[] { this.template };

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
            if (child.Name != this.template.xElement.Name)
            {
                path = child.GetDocumentPath();
                error = $"Expected tag name: '{this.template.xElement.Name}'. Got: '{child.Name}'.";
                return false;
            }

            if (!this.template.MatchesSchema(child, validator, out path, out error))
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
                this.xElement.RemoveNodes();
            }

            return;
        }

        if (attrs.ListMergePolicy == ListMergePolicy.Replace)
        {
            this.xElement.RemoveNodes();
        }
        
        foreach (var child in element.GetFilteredChildren())
        {
            this.xElement.Add(child);
        }
    }

    public override bool Equals(SchemaElement? other)
    {
        if (other is not ListSchemaElement list)
        {
            return false;
        }

        bool thisHasChildren = this.template is not null;
        bool otherHasChildren = list.template is not null;

        if (thisHasChildren != otherHasChildren)
        {
            return false;
        }

        if (!thisHasChildren)
        {
            return true;
        }

        return this.template == list.template;
    }

    public override int GetHashCode()
    {
        return this.template.GetHashCode() ^ "List".GetHashCode();
    }
}
