namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Represents a map, where each member has a different name.
/// </summary>
public class MapSchemaElement : SchemaElement
{
    private readonly Dictionary<XName, SchemaElement> children = new();

    public MapSchemaElement(SchemaElement? parent, XElement node) 
        : base(parent, node)
    {
    }

    public IEnumerable<KeyValuePair<XName, SchemaElement>> Children => this.children;

    public override T Accept<T>(ISchemaVisitor<T> visitor)
    {
        return visitor.Accept(this);
    }

    public void AddChild(SchemaElement child, IErrorCollector errorCollector)
    {
        if (this.children.ContainsKey(child.XmlElement.Name))
        {
            errorCollector.Error(
                "Duplicate tag name under map element",
                child.XmlElement.GetDocumentPath());
        }
        else
        {
            this.children[child.XmlElement.Name] = child;
        }
    }

    protected internal override bool MatchesSchema(
        XElement element,
        MatchKind matchKind,
        IAttributeValidator validator,
        IErrorCollector errorCollector)
    {
        bool returnValue = true;
        validator.Validate(element, errorCollector);

        Dictionary<XName, XElement> map = new();

        foreach (var child in element.GetFilteredChildren())
        {
            if (map.ContainsKey(child.Name))
            {
                errorCollector.Error(
                    "Duplicate tag detected in map element",
                    child.GetDocumentPath());
            }
            else
            {
                map[child.Name] = child;
            }
        }

        foreach (var kvp in map)
        {
            if (this.children.TryGetValue(kvp.Key, out SchemaElement? value))
            {
                returnValue &= value.MatchesSchema(kvp.Value, matchKind, validator, errorCollector);
            }
            else
            {
                errorCollector.Error(
                    "Merged file contains key not present in base file. Merging may not add new keys.",
                    kvp.Value.GetDocumentPath());

                returnValue = false;
            }
        }

        if (matchKind == MatchKind.Strict)
        {
            // We've already validated that the element is a strict subset of the template.
            // However, we now need to do the reverse and validate that the element is a complete
            // subset. We can do this with a simple count, but that produces an error message that is
            // not helpful.
            HashSet<XName> childNames = new(this.children.Where(x => !x.Value.Optional).Select(x => x.Key));

            // childNames now contains things that we expect but are not present in the proposed element.
            childNames.ExceptWith(map.Keys);

            if (childNames.Count > 0)
            {
                errorCollector.Error(
                    $"Schema mismatch. Missing children: {string.Join(",", childNames)}",
                    element.GetDocumentPath());
            }
        }

        return returnValue;
    }

    protected internal override void MergeWithProtected(
        XElement element,
        IAttributeValidator validator,
        IErrorCollector collector)
    {
        Dictionary<XName, XElement> map = element.GetFilteredChildren().ToDictionary(x => x.Name, x => x);

        foreach (var kvp in map)
        {
            SchemaElement value = this.children[kvp.Key];
            value.MergeWithProtected(kvp.Value, validator, collector);
        }
    }
}
