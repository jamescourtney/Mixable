namespace ConfiguratorDotNet.Generator;

internal class ListSchemaElement : SchemaElement
{
    private SchemaElement? template;
    private readonly XName childTagName;

    public ListSchemaElement(
        SchemaElement? parent,
        XElement node,
        XName childTagName) : base(parent, node)
    {
        this.childTagName = childTagName;
    }

    public override IEnumerable<SchemaElement> Children => new[] { this.template };

    public void AddChild(SchemaElement element)
    {
        this.TypeName = $"List<{element.TypeName}>";

        if (this.template is null)
        {
            this.template = element;
        }
        else if (this.template != element)
        {
            throw new ConfiguratorDotNetException($"Children of List '{this.XPath}' have differing schemas.");
        }
    }

    public override void Validate()
    {
        if (this.template is null)
        {
            throw new ConfiguratorDotNetException($"Lists cannot be empty (needed to determine the type). Path = '{this.XPath}'");
        }

        this.template.Validate();
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

        TypeNameStack stack = new();
        stack.Push(this.Parent?.TypeName ?? string.Empty);

        SchemaParser.TryCreateListSchemaElement(
            element,
            this.Parent,
            stack,
            validator,
            out ListSchemaElement? value);

        if (value.template != this.template)
        {
            throw new ConfiguratorDotNetException($"List merging error -- templates do not share the same schema. Path = {this.xElement.GetDocumentPath()}");
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
